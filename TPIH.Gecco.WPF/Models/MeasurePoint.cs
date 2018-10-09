using System;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.Models
{
    public class MeasurePoint 
    {
        public DateTime Date { get; set; }
        public string Reg_Name { get; set; }
        public bool b_val { get; set; }
        public double i_val { get; set; }
        public double ui_val { get; set; }
    }
}
