using System;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.Models
{
    public class MeasurePoint 
    {
        public DateTime Date { get; set; }
        public string Reg_Name { get; set; }
        public double val { get; set; }
        public string data_type { get; set; }
        public string unit { get; set; }
    }
}
