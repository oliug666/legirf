using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Drivers
{
    public class GeccoDriverArgs : EventArgs
    {
        public InfoEnum Info { get; set; }
        public int Value { get; set; }
        public string Message { get; set; }
        public List<MeasurePoint> MeasurePoints { get; set; }
        public uint SetDuration { get; set; }
    }
}
