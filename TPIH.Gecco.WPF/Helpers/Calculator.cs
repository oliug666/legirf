using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Helpers
{
    public class Calculator
    {
        public DataSummary CalculateSummary(List<MeasurePoint> pulse, uint setDuration)
        {
            if (pulse.Count > 10)
            {
                // Factor of 4 to speed up things
                var data = pulse.Skip(4).ToList();
                return new DataSummary
                {
                    Time = DateTime.Now,
                    EstimatedDuration = (uint) (data.Last().Date - data.First().Date).TotalMinutes,
                    AvgB_val = Math.Round(data.Average(d => Convert.ToDouble(d.b_val)), 2),
                    AvgI_val = Math.Round(data.Average(d => d.i_val), 2),
                    AvgUI_val = Math.Round(data.Average(d => d.ui_val), 1)                 
                };
            }
            return null;
        }
    }
}
