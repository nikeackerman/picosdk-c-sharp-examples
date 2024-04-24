﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aquila_RAD
{
    class PL1000Imports
    {
        #region Constants
        private const string _DRIVER_FILENAME = "C:\\Users\\na\\source\\repos\\Aquila_RAD\\Aquila_RAD\\PicoLogger\\pl1000.dll";

        #endregion

        #region Driver Enums

        public enum enPL1000Inputs
        {
            PL1000_CHANNEL_1 = 1,
            PL1000_CHANNEL_2,
            PL1000_CHANNEL_3,
            PL1000_CHANNEL_4,
            PL1000_CHANNEL_5,
            PL1000_CHANNEL_6,
            PL1000_CHANNEL_7,
            PL1000_CHANNEL_8,
            PL1000_CHANNEL_9,
            PL1000_CHANNEL_10,
            PL1000_CHANNEL_11,
            PL1000_CHANNEL_12,
            PL1000_CHANNEL_13,
            PL1000_CHANNEL_14,
            PL1000_CHANNEL_15,
            PL1000_CHANNEL_16,
            PL1000_MAX_CHANNELS = PL1000_CHANNEL_16
        }

        public enum enPL1000DO_Channel
        {
            PL1000_DO_CHANNEL_0,
            PL1000_DO_CHANNEL_1,
            PL1000_DO_CHANNEL_2,
            PL1000_DO_CHANNEL_3,
            PL1000_DO_CHANNEL_MAX
        }

        public enum enPL1000OpenProgress
        {
            PL1000_OPEN_PROGRESS_FAIL = -1,
            PL1000_OPEN_PROGRESS_PENDING = 0,
            PL1000_OPEN_PROGRESS_COMPLETE = 1
        }

        public enum enPL1000Method
        {
            SINGLE,
            WINDOW,
            STREAM,
            MAX_METHOD = STREAM
        }

        #endregion

        #region Driver Imports


        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000CloseUnit")]
        public static extern short CloseUnit(short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000GetUnitInfo")]
        public static extern short GetUnitInfo(short handle,
                                              StringBuilder infoString,
                                              short strlength,
                                              out short reqSize,
                                              int info);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000OpenUnit")]
        public static extern short OpenUnit(out short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000GetSingle")]
        public static extern short Enumerate(short handle,
                                              enPL1000Inputs channel,
                                              out ushort value);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000GetValues")]
        public static extern short GetValue(short handle,
                                            short[] value,
                                            ref ushort noOfValues,
                                            out ushort overflow,
                                            out uint triggerIndex);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000MaxValue")]
        public static extern short MaxValue(short handle,
                                            out ushort maxvalue);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000OpenUnitAsync")]
        public static extern short OpenUnitAsync(out short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000OpenUnitProgress")]
        public static extern short pl1000OpenUnitProgress(out short handle,
                                                            out short progress,
                                                            out short complete);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000Ready")]
        public static extern short pl1000Ready(short handle,
                                                out short ready);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000Run")]
        public static extern short Run(short handle,
                                        uint NoOfValues,
                                        enPL1000Method method);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000SetDo")]
        public static extern short SetDo(short handle,
                                            short do_value,
                                            short DOchannel);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000SetInterval")]
        public static extern short SetInterval(short handle,
                                                ref uint usforblock,
                                                uint IdealNoOfSamples,
                                                short[] channels,
                                                short NoofChannels);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000SetTrigger")]
        public static extern short SetTrigger(short handle,
                                              ushort enabled,
                                              ushort autoTrigger,
                                              ushort autoMS,
                                              enPL1000Inputs channel,
                                              ushort dir,
                                              ushort threshold,
                                              ushort hysteresis,
                                              float delay);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "pl1000Stop")]
        public static extern short stop(short handle);

        #endregion

    }
}
