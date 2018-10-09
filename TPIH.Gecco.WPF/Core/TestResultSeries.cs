using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Core
{
    public class TestResultSeries
    {
        public LineSeries data1 { get; set; }
        public TestResultSeries()
        {
            data1 = new LineSeries("data#1") {YAxisKey = "Primary"};
        }

        /// <summary>
        /// Clear all points inside the LoadResultSeries
        /// </summary>
        public void Clear()
        {
            data1.Points.Clear();
        }

        public void AddPoint(MeasurePoint point)
        {
            data1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(point.Date), Convert.ToDouble(point.b_val)));            
        }
    }
}
