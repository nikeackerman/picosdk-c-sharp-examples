﻿/******************************************************************************
 *
 * Filename: PicoHRDLConsole.cs
 *
 * Description:
 *     This is a Windows Forms application that demonstrates how to use the
 *     PicoLog High Resolution Data Logger (picohrdl) driver functions 
 *     using .NET to collect data on Channel 1 of an ADC-20/24 device.
 *      
 * Supported PicoLog models:
 *  
 *     ADC-20
 *     ADC-24
 *      
 * Copyright © 2015-2017 Pico Technology Ltd. See LICENSE file for terms.
 *      
 ******************************************************************************/

using System;
using System.Windows.Forms;
using System.Threading;

using PicoHRDLImports;

namespace PicoHRDLGui
{
    public partial class PicoHRDLCSGuiForm : Form
    {
        short handle;

        public PicoHRDLCSGuiForm()
        {
            InitializeComponent();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            channel1DataTextBox.Clear();
            numSamplesCollectedTextBox.Clear();

            Thread.Sleep(1000);

            if (handle > 0)
            {
                // Set Input channel 1 - enabled, range = 2500mV, single ended
                short analogChannelStatus = Imports.SetAnalogInChannel(handle, 
                                                                       (short)Imports.HRDLInputs.HRDL_ANALOG_IN_CHANNEL_1, 
                                                                       1,
                                                                       (short)Imports.HRDLRange.HRDL_2500_MV, 
                                                                       1);

                // Set Input channel 1 - enabled, range = 2500mV, single ended
                analogChannelStatus = Imports.SetAnalogInChannel(handle,
                                                                       (short)Imports.HRDLInputs.HRDL_ANALOG_IN_CHANNEL_3,
                                                                       1,
                                                                       (short)Imports.HRDLRange.HRDL_2500_MV,
                                                                       1);

                // Set Interval time = 80 ms, conversion time = 60 ms
                short returnIntervalStatus = Imports.SetInterval(handle, 80, (short)Imports.HRDLConversionTime.HRDL_60MS); 

                // Specify number of values to collect and capture block of data
                int numSamplesPerChannel = 0;
                
                Int32.TryParse(numSamplesPerChannelTextBox.Text, out numSamplesPerChannel);

                short status = Imports.HRDLRun(handle, numSamplesPerChannel, (short)Imports.BlockMethod.HRDL_BM_BLOCK);

                short ready = Imports.HRDLReady(handle);

                while (ready != 1)
                {
                    ready = Imports.HRDLReady(handle);
                    Thread.Sleep(100);
                }

                short stopStatus = Imports.HRDLStop(handle);

                // Get data values
                short numActiveChannels = 0;

                short numAnalogueChannelsStatus = Imports.GetNumberOfEnabledChannels(handle, out numActiveChannels);

                int[] data = new int[numActiveChannels * numSamplesPerChannel];
                short overflow = 0;

                int numSamplesCollectedPerChannel = Imports.GetValues(handle, data, out overflow, numSamplesPerChannel);

                // Get Max Min ADC Count values for Channel 1
                int minAdc = 0;
                int maxAdc = 0;

                short returnAdcMaxMin = Imports.GetMinMaxAdcCounts(handle, 
                                                                   out minAdc, 
                                                                   out maxAdc, 
                                                                   (short)Imports.HRDLInputs.HRDL_ANALOG_IN_CHANNEL_1);

                // Display retreived data
                numSamplesCollectedTextBox.Text += numSamplesCollectedPerChannel.ToString();

                float[] scaledData = new float[numSamplesCollectedPerChannel * numActiveChannels];

                for (int n = 0; n < numSamplesCollectedPerChannel; n++)
                {
                    scaledData[n] = adcToMv(data[n], (short)Imports.HRDLRange.HRDL_2500_MV, maxAdc);
                    channel1DataTextBox.Text += "Raw: " + data[n] + "\tScaled: " + scaledData[n] + "\r\n";
                }
            }
            else
            {
                MessageBox.Show("No connection to device.");
            }
        }

        /**
         * GetDeviceInfo 
         * 
         * Prints information about the device to the console window.
         * 
         * Inputs:
         *      handle - the handle to the device
         */
        public void GetDeviceInfo(short handle)
        {
            string[] description = {
                           "Driver Version    ",
                           "USB Version       ",
                           "Hardware Version  ",
                           "Variant Info      ",
                           "Serial            ",
                           "Cal Date          ",
                           "Kernel Ver        "
                         };

            System.Text.StringBuilder line = new System.Text.StringBuilder(80);

            if (handle >= 0)
            {
                for (short i = 0; i < 6; i++)
                {

                    Imports.GetUnitInfo(handle, line, 80, i);

                    unitInfoTextBox.Text += description[i] + ": " + line.ToString() + "\r\n";
                }
            }

        }

        /**
         * adcToMv 
         * 
         * 
         */
        public float adcToMv(int value, short range, int maxValue)
        {
            float mvValue = 0.0f;

            float vMax = (float)(Imports.MAX_VOLTAGE_RANGE / Math.Pow(2, range)); // Find voltage scaling factor

            mvValue = ((float)value / maxValue) * vMax;

            return mvValue;
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            // Clear text boxes
            unitInfoTextBox.Clear();

            handle = Imports.HRDLOpenUnit();

            GetDeviceInfo(handle);

            // Set Mains Rejection

            short setMainsStatus = Imports.SetMains(handle, Imports.HRDLMainsRejection.HRDL_FIFTY_HERTZ);   // Set noise rejection for 50Hz  
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            short closeStatus = Imports.HRDLCloseUnit(handle);

            System.Windows.Forms.Application.Exit();
        }

    }
}
