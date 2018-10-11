using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Helpers
{
    public static class Plotter
    {
        public static string PRIMARY_AXIS = "Primary";
        public static string SECONDARY_AXIS = "Secondary";

        public static void ShowPoints(IList<MeasurePoint> points, PlotModel wPlot, string AxisKey)
        {
            if (points != null && points.Any() && points.All(p => p != null))
            {
                LineSeries newSerie = new LineSeries
                {
                    Title = points[0].Reg_Name,
                    CanTrackerInterpolatePoints = false
                };

                AddPoints(newSerie, points);
                newSerie.YAxisKey = AxisKey;

                wPlot.Series.Add(newSerie);
                wPlot.InvalidatePlot(true);
            }
        }

        public static void AnnotateAlarms(PlotModel WPlot, List<MeasurePoint> AlarmValueList, string Annotation)
        {
            // Check when the alarm was triggered and when it was gone
            // var toPlot = DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList();
            var where_active = AlarmValueList.Where(x => x.val == 1).ToList();
            var where_inactive = AlarmValueList.Where(x => x.val == 0).ToList();
            foreach (MeasurePoint MP in where_active)
            {
                WPlot.Annotations.Add(new LineAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = DateTimeAxis.ToDouble(MP.Date),
                    Color = OxyPlot.OxyColors.Red,
                    Text = Annotation,
                    ClipByXAxis = true
                });
            }
            foreach (MeasurePoint MP in where_inactive)
            {
                WPlot.Annotations.Add(new LineAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = DateTimeAxis.ToDouble(MP.Date),
                    Color = OxyColors.Green,
                    Text = Annotation,
                    ClipByXAxis = true
                });
            }
            WPlot.InvalidatePlot(true);
        }
    
        private static void AddPoints(LineSeries ls, IList<MeasurePoint> myPoints)
        {
            if (myPoints != null && myPoints.Any() && myPoints.All(p => p != null))
            {
                ls.Points.Clear();
                foreach (MeasurePoint mp in myPoints)
                {
                    ls.Points.Add(new DataPoint(DateTimeAxis.ToDouble(mp.Date), mp.val));
                }
            }
        }
    }
}
