using System;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.Models
{
    public class DataSummary
    {
        public DateTime Time { get; set; }
        public uint EstimatedDuration { get; set; }
        public double AvgB_val { get; set; }
        public double AvgI_val { get; set; }
        public double AvgUI_val { get; set; }
    }
}
