using System.Collections.Generic;

namespace TPIH.Gecco.WPF.Models
{
    public class MeasurementCollection
    {
        public int Count
        {
            get { return Measurements != null ? Measurements.Count : 0; }
        }

        public List<PulseMeasurement> Measurements { get; set; } 
    }
}
