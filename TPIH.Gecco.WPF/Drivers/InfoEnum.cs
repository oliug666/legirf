using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPIH.Gecco.WPF.Drivers
{
    public enum InfoEnum
    {
        DurationInfo,
        PowerInfo,
        DelayInfo,
        SideInfo,
        SinglePulse,
        ContinousPulseStart,
        ContinousPulseStop,
        SetPower,
        SetSideLeft,
        SetSideRight,
        SetSideBoth,
        SetContinousDelay,
        SetPulseDuration,
        StatusMessage,
        CANData,
        MonitorCAN,
        GeneratorError,
        PulseStarted,
        PulseDisabled,
        FirmwareUpdate,
        PulseHigh,
        PulseLow
    }
}
