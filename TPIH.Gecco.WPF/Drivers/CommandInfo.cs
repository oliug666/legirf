using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPIH.Gecco.WPF.Drivers
{
    public struct CommandInfo
    {
        public static string DurationInfo = "DUR";
        public static string PowerInfo = "PWR";
        public static string DelayInfo = "DEL";
        public static string SideInfo = "SIDE";
        public static string SinglePulse = "SIGP";
        public static string ContinousPulseStart = "CONP";
        public static string ContinousPulseStop = "CONS";
        public static string SetPower = "SP";
        public static string SetSideLeft = "SIDL";
        public static string SetSideRight = "SIDR";
        public static string SetSideBoth = "SIDB";
        public static string SetContinousDelay = "SDE";
        public static string SetPulseDuration = "SDU";
        public static string StatusMessage = "MSG";
        public static string CANData = "CAN";
        public static string MonitorCAN = "MON";
        public static string GeneratorError = "GENE";
        public static string PulseStarted = "PULS";
        public static string PulseDisabled = "PULD";
        public static string FirmwareUpdate = "UPDA";
        public static string PulseHigh = "PULH";
        public static string PulseLow = "PULL";

    }
}
