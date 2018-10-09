using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPIH.Gecco.WPF.Models
{
    public class GeneratorErrorModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }

        public GeneratorErrorModel()
        {
            Date = DateTime.Now;
            Description = "Unkown";
        }
    }
}
