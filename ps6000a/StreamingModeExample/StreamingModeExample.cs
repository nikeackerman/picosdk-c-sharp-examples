﻿// This is an example of how to setup and capture in streaming mode for PicoScope 6000 Series PC Oscilloscope consuming the ps6000a driver

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DriverImports;
using PicoPinnedArray;


namespace StreamingModeExample
{
    public struct ChannelSettings
    {
        public ChannelRange range;
        public bool enabled;
    }
    class StreamingModeExample
  {
        /// <summary>
        /// Streaming code N BufferSets can be defined
        /// </summary>
        ///
        public static int numChannels;
        private static ps6000aDevice.ChannelSettings[] _channelSettings = new ps6000aDevice.ChannelSettings[10];
        static StandardDriverStatusCode RunStreamingModeExample(short handle, DeviceResolution resolution)
        {
            //Define sampling Settings ( Currently - 0.1 Second per BufferSet (1us x 100k ) )
            ulong numSamples = 100000;//100kS
            double idealTimeInterval = 1;
            uint sampleIntervalTimeUnits = (uint)PicoTimeUnits.us;

            //Setup Channels
            Console.WriteLine("\nCHANGE ENABLED CHANNELS IN Main() as required!");
            int NoEnabledchannels;
            var status = ps6000aDevice.InitializeChannelsAndRanges(handle, in _channelSettings, numChannels, out NoEnabledchannels);
            if (status != StandardDriverStatusCode.Ok) return status;
            Console.WriteLine("\n");
            DriverImports.Action Action_temp = DriverImports.Action.PICO_CLEAR_ALL | DriverImports.Action.PICO_ADD;

            //Create all buffers for each memory segment and channel
            //Set the number buffers needed (2 or greater)
            int memorysegments = 3;
            // Set up the data arrays and pin them (3D Array - [Buffer set],[Channel],[Sample index] )
            short[][][] values = new short[memorysegments][][];
            PinnedArray<short>[,] pinned = new PinnedArray<short>[memorysegments, numChannels];

            for (ushort segment = 0; segment < memorysegments; segment++)
            {
                values[segment] = new short[numChannels][];
                Console.WriteLine("Creating pinned Arrays for bufferSet " + segment);

                for (short channel = 0; channel < numChannels; channel++)
                {
                    if (_channelSettings[channel].enabled)
                    {
                        values[segment][channel] = new short[numSamples];
                        pinned[segment, channel] = new PinnedArray<short>(values[segment][channel]);
                        
                        if (segment == 0)// Set the first data buffer for each channel
                        {
                            status = PS6000a.SetDataBuffers(handle,
                             (Channel)channel,
                             values[0][channel],
                             null,
                             (int)numSamples,
                             DataType.PICO_INT16_T,
                             0,
                             RatioMode.PICO_RATIO_MODE_RAW,
                             Action_temp);
                            Action_temp = DriverImports.Action.PICO_ADD;//set to "ADD" for all other calls

                            if (status != StandardDriverStatusCode.Ok)
                            {
                                Console.WriteLine("\nError from function SetDataBuffers with status: " + status);
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Calling SetDataBuffer() BufferSet 0 for " + (Channel)channel);
                            }
                        }
                    }
                }
            }

            short autostop = 0;
            // Start continuous streaming
            Console.WriteLine("\nStarting Data Capture...");
            ulong noOfPreTriggerSamples = 0;
            Console.WriteLine("\nNumber of PreTriggerSamples " + noOfPreTriggerSamples);
            status = PS6000a.RunStreaming(handle,
                 out idealTimeInterval,
                 sampleIntervalTimeUnits,
                 noOfPreTriggerSamples,
                 numSamples - noOfPreTriggerSamples,
                 autostop,
                 (ulong)1,
                 RatioMode.PICO_RATIO_MODE_RAW);
            Console.WriteLine("\nRunStreaming sample time is " + idealTimeInterval + (PicoTimeUnits)sampleIntervalTimeUnits );
            Console.WriteLine("Total number of samples is {0}", numSamples);
            Console.WriteLine("Autostop is: {0}\n" , autostop);

            if (status != StandardDriverStatusCode.Ok)
            {
                Console.WriteLine("\nError from function RunStreaming with status: " + status);
            }

            //Create Arrays of Structs for GetStreamingLatestValues for each memory segment
            StreamingDataTriggerInfo[] streamingDataTriggerInfoArray = new StreamingDataTriggerInfo[memorysegments];
            StreamingDataTriggerInfo streamingDataTriggerInfoTemp = new StreamingDataTriggerInfo();

            StreamingDataInfo[,] streamingDataInfoArray = new StreamingDataInfo[memorysegments, NoEnabledchannels];
            StreamingDataInfo[] streamingDataInfoTempArray = new StreamingDataInfo[NoEnabledchannels];

            StreamingDataTriggerInfo StreamingDataTriggerInfo0 = new StreamingDataTriggerInfo //(ulong triggerAt, short triggered, short autoStop)
            (0, 0, 0);
            streamingDataTriggerInfoTemp = StreamingDataTriggerInfo0;

            //Fill both Arrays with default struct vaules
            for (int j = 0; j < memorysegments; j++)
            {
                //streamingDataInfoArray[j] = StreamingDataInfo0;
                streamingDataTriggerInfoArray[j] = StreamingDataTriggerInfo0;
            }

            for (int j = 0; j < (memorysegments); j++)
            {
                for (short channel = 0; channel < numChannels; channel++)
                {
                    if (_channelSettings[channel].enabled)
                    {//Set default vaules for each struct and set correct channel value
                        streamingDataInfoArray[j, channel].Channel = (Channel)channel;//
                        streamingDataInfoArray[j, channel].Mode = RatioMode.PICO_RATIO_MODE_RAW;
                        streamingDataInfoArray[j, channel].Type = DataType.PICO_INT16_T;//
                        streamingDataInfoArray[j, channel].NoOfSamples = 0;//
                        streamingDataInfoArray[j, channel].BufferIndex = 0;//
                        streamingDataInfoArray[j, channel].StartIndex = 0;//
                        streamingDataInfoArray[j, channel].Overflow = 0;//
                    }
                }
            }

            StreamingDataInfo[] dataStreamInfo = new StreamingDataInfo[NoEnabledchannels];
            ///Copy Enabledchannels to temp StreamingDataInfo
            for (int j = 0; j < NoEnabledchannels; j++)
            {
                dataStreamInfo[j] = streamingDataInfoArray[0, j];
            }

            if (status == StandardDriverStatusCode.Ok)
            {
                bool SetDataBufferFlag = false;
                int i = 0;

                while (i < memorysegments) //loop for each buffer Set created
                {
                    //SetDataBufferFlag = true;
                    //Allocate all array elements to driver (with SetDataBuffers() )
                    if (SetDataBufferFlag)
                    { 
                        for (short channel = 0; channel < numChannels; channel++)
                        {
                            if (_channelSettings[channel].enabled)
                            {
                                status = PS6000a.SetDataBuffers(handle,
                                (Channel)channel,
                                values[i][channel], //i is the memory segment index
                                null,               //not using downsampling buffers passing null
                                (int)numSamples,
                                DataType.PICO_INT16_T,
                                0,
                                RatioMode.PICO_RATIO_MODE_RAW,
                                Action_temp);
                                Console.WriteLine("Calling SetDataBuffer() BufferSet " + i + " for " + (Channel)channel + " ");
                                if (status != StandardDriverStatusCode.Ok)
                                {
                                    Console.WriteLine("\nError from function SetDataBuffers with status: " + status);
                                    break;
                                }
                            }
                        }
                    }
                    SetDataBufferFlag = false;

                    //delay millseconds for driver to fill channel buffer(s)
                    //(timeInternal x SI units x samples x 1000) x 0.3 delay in ms to fill buffer 30% (Recommend delay is 30-50%)
                    double timedelay = (double)( (idealTimeInterval * (Math.Pow(10, 3 * sampleIntervalTimeUnits) / 1E+15)) * numSamples * 0.3 * 1000);
                    Thread.Sleep((int)timedelay); 

                    //Call GetStreamingLatestValues() - passing buffer status data in and out
                    //marshal Array of active channels strutures into pointer
                    var size = Marshal.SizeOf(typeof(StreamingDataInfo));
                    IntPtr pDataInfoValues = Marshal.AllocHGlobal(size * dataStreamInfo.Length);
                    //
                    try
                    { 
                        IntPtr pnt = new IntPtr(pDataInfoValues.ToInt64());

                        for (int a = 0; a < dataStreamInfo.Length; a++)
                        {
                            Marshal.StructureToPtr(dataStreamInfo[a], pnt, true);
                            pnt = new IntPtr(pnt.ToInt64() + size);
                        }

                        status = PS6000a.GetStreamingLatestValues(handle,
                        pDataInfoValues, //pointer to dataStreamInfo,
                         (ulong)dataStreamInfo.Length,
                        ref streamingDataTriggerInfoTemp);

                        IntPtr ptr = pDataInfoValues;
                        for (int b = 0; b < dataStreamInfo.Length; b++)
                        {
                            //marshal the pointer to an array of StreamingDataInfo (out)
                            StreamingDataInfo StreamingDataInfo =
                                (StreamingDataInfo)Marshal.PtrToStructure(ptr, typeof(StreamingDataInfo));
                            dataStreamInfo[b] = StreamingDataInfo;
                            ptr += Marshal.SizeOf(typeof(StreamingDataInfo));
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pDataInfoValues);
                    }
                    ///Copy returned Array and sturture to Arrays for each segement
                    for (int j = 0; j < NoEnabledchannels; j++)
                    {
                        streamingDataInfoArray[i, j] = dataStreamInfo[j];
                    }
                    streamingDataTriggerInfoArray[i] = streamingDataTriggerInfoTemp;

                    Console.WriteLine("\nPolling Delay is: " + timedelay + "ms");
                    Console.WriteLine("Polling GetStreamingLatestValues status = " + status + " noOfSamples: " + streamingDataInfoArray[i, 0].NoOfSamples + " \t-StartIndex: " + streamingDataInfoArray[i, 0].StartIndex);
                    // If buffers full move to next bufferSet
                    if (status == StandardDriverStatusCode.PICO_WAITING_FOR_DATA_BUFFERS)//driver waiting for more buffers
                    {
                        Console.WriteLine(" ");
                        if (streamingDataTriggerInfoArray[i].AutoStop == 1)//exit loop and stop
                            break;
                        i++;//index next bufferSet
                        SetDataBufferFlag = true;
                    }
                    else
                    {
                        if (status != StandardDriverStatusCode.Ok)
                        {
                            Console.WriteLine("\nError from function GetStreamingLatestValues with status: " + status);
                            break;
                        }
                    }
                }
                //Write each segment to a file
                ps6000aDevice.WriteArrayToFiles(values, _channelSettings,
                                                (double) idealTimeInterval * (Math.Pow(10, 3 * sampleIntervalTimeUnits) / 1E+15),//sample interval
                                                "Streaming BufferSet",
                                                (short)noOfPreTriggerSamples);//Trigger point if set in first BufferSet
                Console.WriteLine("\n");
            }
            return status;
    }

    static void Main(string[] args)
    {
        short handle = 0;
        var resolution = DeviceResolution.PICO_DR_8BIT;
        short MinValues, MaxValues = 0;
        DriverImports.StandardDriverStatusCode status = 0;
        ////////////////////////////////////////////////  Enabled/Disable channels as required!
        _channelSettings[0].enabled = true;//ChA
        _channelSettings[1].enabled = true;//ChB
        _channelSettings[2].enabled = true;//ChC
        _channelSettings[3].enabled = true;//ChD
        _channelSettings[4].enabled = false;//ChE
        _channelSettings[5].enabled = false;//ChF
        _channelSettings[6].enabled = false;//ChG
        _channelSettings[7].enabled = false;//ChH
        ///////////////////////////////////////////

            status = ps6000aDevice.OpenUnit(out handle, resolution, out numChannels);

        status = PS6000a.GetAdcLimits(handle, resolution, out MinValues, out MaxValues);
        if (status != StandardDriverStatusCode.Ok)
        {
            Console.WriteLine("GetAdcLimits returned status: " + status);
        }
        Console.WriteLine("GetAdcLimits() returned MaxValues: " + MaxValues);

        if (status == StandardDriverStatusCode.Ok)
        {
            status = RunStreamingModeExample(handle, resolution);
            Console.WriteLine("RunStreamingModeExample exited with status: " + status);
        }

        status = PS6000a.Stop(handle);
        Console.WriteLine("Stopping unit with status: " + status);
        status = PS6000a.CloseUnit(handle);
        Console.WriteLine("Closed unit with status: " + status);
        Console.ReadLine();
    }
  }
}
