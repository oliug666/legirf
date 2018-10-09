using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPIH.Gecco.WPF.Models
{
    public class PulseMeasurement
    {
        public DateTime Date { get; set; }
        public List<MeasurePoint> Points { get; set; }
        
    }
}
