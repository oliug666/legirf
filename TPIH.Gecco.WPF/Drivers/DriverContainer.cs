using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.Annotations;
using TPIH.Gecco.WPF.Interfaces;

namespace TPIH.Gecco.WPF.Drivers
{
    public static class DriverContainer
    {
        private static IGeccoDriver _driver;
        public static IGeccoDriver Driver
        {
            get { return _driver ?? (_driver = new GeccoDriver()); }
            set { _driver = value; }
        }
    }
}
