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

        public static void ShowAnnotations(IList<string> alarmNames, PlotModel pM, bool description)
        {
            if (DriverContainer.Driver.MbAlarm != null)
            {
                lock (DriverContainer.Driver.MbAlarm)
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

                                ShowAlarms(pM, DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList(),
                                annotationText);
                            }
                        }
                    }
                }
            }
        }

        public static void ClearAnnotations(PlotModel wPlot)
        {
            wPlot.Annotations.Clear();
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

        public static List<bool> AreThereActiveAlarms(IList<string> alarmNames)
        {
            List<bool> alarmActiveFlags = new List<bool>();

            if (alarmNames != null)
            {
                if (alarmNames.Count > 0)
                {
                    foreach (string sg in alarmNames)
                    {
                        var aList = getAlarmListThreadSafe(sg);
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

        private static List<MeasurePoint> getAlarmListThreadSafe(string name)
        {
            if (DriverContainer.Driver.MbAlarm != null)
            {
                lock (DriverContainer.Driver.MbAlarm)
                {
                    return DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList();
                }
            }

            return null;
        }
    }
}
