using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;
using TPIH.Gecco.WPF.ViewModels;

namespace TPIH.Gecco.WPF.Helpers
{
    public class MultiPlotViewModel : ViewModelBase
    {
        private bool _showAlarms = true;

        public List<string> RegNames00, RegNames01, RegNames10, RegNames11;
        public List<string> RegDescriptions00, RegDescriptions01, RegDescriptions10, RegDescriptions11;
        public List<string> RegUnits00, RegUnits01, RegUnits10, RegUnits11;

        private Visibility _isFileLoaded;
        public Visibility IsFileLoaded { get { return _isFileLoaded; } set { _isFileLoaded = value; OnPropertyChanged(() => IsFileLoaded); } }

        private PlotModel _p00, _p01, _p10, _p11;
        public PlotModel Plot00
        {
            get { return _p00; }
            set
            {
                _p00 = value;
                OnPropertyChanged(() => Plot00);
            }
        }
        public PlotModel Plot01
        {
            get { return _p01; }
            set
            {
                _p01 = value;
                OnPropertyChanged(() => Plot01);
            }
        }
        public PlotModel Plot10
        {
            get { return _p10; }
            set
            {
                _p10 = value;
                OnPropertyChanged(() => Plot10);
            }
        }
        public PlotModel Plot11
        {
            get { return _p11; }
            set
            {
                _p11 = value;
                OnPropertyChanged(() => Plot11);
            }
        }

        public void ClearAll(PlotModel pM)
        {
            Plotter.ClearAnnotations(pM);
            Plotter.ClearPoints(pM);
            pM.InvalidatePlot(true);
        }

        public void AddSeries(PlotModel pM, IList<string> RegNames)
        {
            if (RegNames != null)
            {
                if (RegNames.Count > 0)
                {
                    pM.Series.Clear();
                    pM.Annotations.Clear();
                    foreach (string regName in RegNames)
                    {
                        if (regName != " ")
                        {
                            var myPoints = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == regName).ToList();
                            ShowPoints(myPoints, pM);
                        }
                    }
                    pM.InvalidatePlot(true);
                }
            }
        }
        public void ShowPoints(IList<MeasurePoint> points, PlotModel pM)
        {
            if (IsFileLoaded == Visibility.Visible)
            {
                if (points != null && points.Any() && points.All(p => p != null))
                {
                    // Draw plot
                    if (points[0].unit == N3PR_Data.PERCENTAGE)
                        Plotter.ShowPoints(points, pM, Plotter.SECONDARY_AXIS);
                    else
                        Plotter.ShowPoints(points, pM, Plotter.PRIMARY_AXIS);

                    // Annotate Alarms
                    if (_showAlarms)
                    {
                        if (DriverContainer.Driver.MbAlarm != null)
                        {
                            List<string> alarmNames = DriverContainer.Driver.MbAlarm.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                            Plotter.ShowAnnotations(alarmNames, pM, true);
                        }
                    }
                }
            }
        }
        public List<string> GetRegDescription(List<string> RegNames)
        {
            List<string> RegDescriptions = new List<string>();
            if (RegNames != null)
            {
                foreach (string st in RegNames)
                {
                    if (N3PR_Data.REG_NAMES.IndexOf(st) != -1)
                        RegDescriptions.Add(N3PR_Data.REG_DESCRIPTION[N3PR_Data.REG_NAMES.IndexOf(st)]);
                    else
                        RegDescriptions.Add(" ");
                }
                return RegDescriptions;
            }
            else
                return null;
        }

        public List<string> GetRegUnits(List<string> RegNames)
        {
            List<string> RegUnits = new List<string>();
            if (RegNames != null)
            {
                foreach (string st in RegNames)
                {
                    if (N3PR_Data.REG_NAMES.IndexOf(st) != -1)
                        RegUnits.Add(N3PR_Data.REG_MEASUNIT[N3PR_Data.REG_NAMES.IndexOf(st)]);
                    else
                        RegUnits.Add(" ");
                }
            }
            return RegUnits;
        }
        public PlotModel CreatePlotModel(List<string> RegDescriptions, List<string> RegUnits)
        {
            PlotModel pM;

            if (RegDescriptions != null)
            {
                if (RegDescriptions.Count() > 0)
                {
                    if (RegDescriptions.Count() == 1)
                        pM = new PlotModel(RegDescriptions[0]);
                    else
                        pM = new PlotModel("");
                }
                else
                    return null;
            }
            else
                return null;

            pM.Axes.Clear();
            pM.Axes.Add(new DateTimeAxis(AxisPosition.Bottom, "Time")
            {
                Key = "X",
                StringFormat = "dd-MM-yyyy hh:mm",
                IsZoomEnabled = true,
                IntervalLength = 100,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                IsPanEnabled = false
            });
            pM.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = true
            });

            if (RegUnits != null)
            {
                if (RegUnits.Contains(N3PR_Data.PERCENTAGE))
                    pM.Axes.Add(new LinearAxis(AxisPosition.Right, N3PR_Data.PERCENTAGE)
                    {
                        Key = "Secondary",
                        MajorGridlineStyle = LineStyle.Solid,
                        Minimum = 0,
                        Maximum = 100,
                        MajorGridlineColor = OxyColors.LightBlue,
                        TicklineColor = OxyColors.LightBlue,
                        TitleColor = OxyColors.Blue,
                        TextColor = OxyColors.Blue,
                        IsZoomEnabled = true
                    });
            }
            return pM;
        }

        public void OnFlaggedAlarmMessageReceived(ItemCheckedEvent e)
        {
            if (e.value) // show annotations
            {
                _showAlarms = true;
                // Refresh Annotations
                Plotter.ClearAnnotations(Plot00);
                Plotter.ClearAnnotations(Plot01);
                Plotter.ClearAnnotations(Plot10);
                Plotter.ClearAnnotations(Plot11);
                if (DriverContainer.Driver.MbAlarm != null)
                {
                    List<string> alarmNames = DriverContainer.Driver.MbAlarm.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                    Plotter.ShowAnnotations(alarmNames, Plot00, true);
                    Plotter.ShowAnnotations(alarmNames, Plot01, true);
                    Plotter.ShowAnnotations(alarmNames, Plot10, true);
                    Plotter.ShowAnnotations(alarmNames, Plot11, true);
                }
            }
            else // unshow annotations
            {
                _showAlarms = false;
                Plotter.ClearAnnotations(Plot00);
                Plotter.ClearAnnotations(Plot01);
                Plotter.ClearAnnotations(Plot10);
                Plotter.ClearAnnotations(Plot11);
            }
            Plot00.InvalidatePlot(true);
            Plot01.InvalidatePlot(true);
            Plot10.InvalidatePlot(true);
            Plot11.InvalidatePlot(true);
        }
    }
}
