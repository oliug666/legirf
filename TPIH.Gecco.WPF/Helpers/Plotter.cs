using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
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
                string reg_description = N3PR_Data.REG_DESCRIPTION[N3PR_Data.REG_NAMES.IndexOf(points[0].Reg_Name)];

                LineSeries newSerie = new LineSeries
                {
                    Title = points[0].Reg_Name,
                    CanTrackerInterpolatePoints = false,
                    TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.##}\n" + reg_description
                };
                /*
                {0} = Title of Series
                {1} = Title of X-Axis
                {2} = X Value
                {3} = Title of Y-Axis
                {4} = Y Value
                */
                AddPoints(newSerie, points);
                newSerie.YAxisKey = AxisKey;
                wPlot.Series.Add(newSerie);
            }
        }

        public static void UnshowPoints(PlotModel wPlot, string seriesName)
        {
            List<Series> tdbSerie = wPlot.Series.Where(x => x.Title == seriesName).ToList();
            if (tdbSerie.Count != 0)
            {
                foreach (var _tbd in tdbSerie)
                {
                    wPlot.Series.Remove(_tbd);
                }
            }
        }
        public static void ClearPoints(PlotModel wPlot)
        {
            if (wPlot.Series.Count != 0)
            {
                wPlot.Series.Clear();                
            }
        }

        public static void ShowAnnotations(IList<string> alarmNames, IList<MeasurePoint> mbAlarms, PlotModel pM, bool description)
        {
            if (mbAlarms != null)
            {
                if (alarmNames.Count > 0)
                {
                    // Annotate the plot just one time
                    List<Annotation> Annotations = pM.Annotations.ToList();
                    if (Annotations.Count == 0)
                    {
                        foreach (string name in alarmNames)
                        {
                            string annotationText = "";
                            if (description)
                                annotationText = N3PR_Data.ALARM_DESCRIPTION[N3PR_Data.ALARM_WARNING_NAMES.IndexOf(name)];

                            ShowAlarms(pM, mbAlarms.Where(x => x.Reg_Name == name).ToList(),
                            annotationText);
                        }
                    }
                }
            }
        }

        public static void ClearAnnotations(PlotModel wPlot)
        {
            wPlot.Annotations.Clear();
        }

        public static void RefreshAnnotations(IList<MeasurePoint> mbAlarms, PlotModel pM, bool annotation)
        {
            // Refresh Annotations
            if (mbAlarms != null)
            {
                ClearAnnotations(pM);
                List<string> alarmNames = mbAlarms.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                ShowAnnotations(alarmNames, mbAlarms, pM, annotation);
            }
        }

        private static void ShowAlarms(PlotModel WPlot, List<MeasurePoint> AlarmValueList, string Annotation)
        {
            List<DateTime> WhereAlreadyAnnotated = new List<DateTime>();
            foreach (Annotation ann in WPlot.Annotations.ToList())
            {
                TooltipAnnotation tann = (TooltipAnnotation)ann;
                WhereAlreadyAnnotated.Add(DateTimeAxis.ToDateTime(tann.X));
            }

            VerticalAlignment va;
            // Check when the alarm was triggered and when it was gone
            // var toPlot = DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList();
            var where_active = AlarmValueList.Where(x => x.val == 1).ToList();
            var where_inactive = AlarmValueList.Where(x => x.val == 0).ToList();
            foreach (MeasurePoint MP in where_active)
            {
                if (WhereAlreadyAnnotated.Contains(MP.Date))
                    va = VerticalAlignment.Bottom;
                else
                    va = VerticalAlignment.Top;

                if (N3PR_Data.ALARM_NAMES.Contains(MP.Reg_Name))
                {
                    WPlot.Annotations.Add(new TooltipAnnotation
                    {
                        Type = LineAnnotationType.Vertical,
                        X = DateTimeAxis.ToDouble(MP.Date),
                        Color = OxyPlot.OxyColors.Red,
                        StrokeThickness = 2,
                        Text = Annotation,
                        ClipByXAxis = true,
                        TextVerticalAlignment = va,
                        Tooltip = Annotation + "\nTime: " + MP.Date.ToString(N3PR_Data.DATA_FORMAT)
                        + "\nValue: 1"
                    });
                }
                else
                {
                    WPlot.Annotations.Add(new TooltipAnnotation
                    {
                        Type = LineAnnotationType.Vertical,
                        X = DateTimeAxis.ToDouble(MP.Date),
                        Color = OxyPlot.OxyColors.Orange,
                        StrokeThickness = 2,
                        Text = Annotation,
                        ClipByXAxis = true,
                        TextVerticalAlignment = va,
                        Tooltip = Annotation + "\nTime: " + MP.Date.ToString(N3PR_Data.DATA_FORMAT)
                        + "\nValue: 1"
                    });
                }

                WhereAlreadyAnnotated.Add(MP.Date);
            }
            foreach (MeasurePoint MP in where_inactive)
            {
                if (WhereAlreadyAnnotated.Contains(MP.Date))
                    va = VerticalAlignment.Bottom;
                else
                    va = VerticalAlignment.Top;

                WPlot.Annotations.Add(new TooltipAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = DateTimeAxis.ToDouble(MP.Date),
                    Color = OxyColors.Green,
                    StrokeThickness = 2,
                    Text = Annotation,
                    ClipByXAxis = true,
                    TextVerticalAlignment = va,
                    Tooltip = Annotation + "\nTime: " + MP.Date.ToString(N3PR_Data.DATA_FORMAT)
                    + "\nValue: 0"
                });

                WhereAlreadyAnnotated.Add(MP.Date);
            }
        }
    
        public static void RefreshSeries(IList<MeasurePoint> mbData, IList<Series> tbrSeries)
        {
            if (mbData != null)
            {
                foreach (Series tbrSerie in tbrSeries)
                {
                    var ls = (LineSeries)tbrSerie;
                    var myPoints = mbData.Where(x => x.Reg_Name == ls.Title).ToList();
                    AddPoints(ls, myPoints);
                }
            }           
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

        public static List<bool> AreThereActiveAlarms(IList<string> alarmNames, IList<MeasurePoint> mbAlarms)
        {
            List<bool> alarmActiveFlags = new List<bool>();

            if (alarmNames != null)
            {
                if (alarmNames.Count > 0)
                {
                    foreach (string sg in alarmNames)
                    {
                        var aList = mbAlarms.Where(x => x.Reg_Name == sg).ToList();
                        if (aList != null)
                        {
                            // Find the latest date
                            List<DateTime> latestDates = aList.Select(p => p.Date).ToList();
                            int whereMaxDate = latestDates.IndexOf(latestDates.Max());
                            // At that time, was the alarm/warning active?
                            alarmActiveFlags.Add(aList[whereMaxDate].val == 0 ? false : true);
                        }
                    }
                    return alarmActiveFlags;
                }
            }

            return null;
        }

        public static void OnMouseDown(object sender, OxyMouseEventArgs e, PlotModel pM)
        {
            // If we are inside the legend
            OxyRect LegArea = pM.LegendArea;
            if (LegArea.Contains(e.Position.X, e.Position.Y))
            {
                int selectedSerie = -1;
                // Check the number of series
                var sCount = pM.Series.Count();
                double deltaY = (LegArea.Bottom - LegArea.Top) / sCount;
                for (int i = 0; i < sCount; i++)
                {
                    // We selected the i-th series
                    if (e.Position.Y >= (LegArea.Top + i * deltaY) && e.Position.Y < (LegArea.Top + (i + 1) * deltaY))
                    {
                        selectedSerie = i;
                    }
                }

                if (selectedSerie != -1)
                {
                    var LineSerie = (LineSeries)pM.Series[selectedSerie];
                    if (LineSerie.StrokeThickness == 2)
                        LineSerie.StrokeThickness = 3;
                    else
                        LineSerie.StrokeThickness = 2;
                }
            }

            pM.InvalidatePlot(true);
        }
    }
}
