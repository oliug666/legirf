using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPIH.Gecco.WPF.Helpers
{
    public class ItemCheckedEvent
    {
        public string name { get; set; }
        public bool value { get; set; }
        public string data_type { get; set; }
        public ItemCheckedEvent(string n, bool v, string d)
        {
            name = n;
            value = v;
            data_type = d;
        }
    }
}
