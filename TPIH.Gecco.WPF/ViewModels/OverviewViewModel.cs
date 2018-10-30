using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class OverviewViewModel : ViewModelBase
    {
        private List<string> _regNames00, _regNames01, _regNames10, _regNames11;
        private List<string> _regDescriptions00, _regDescriptions01, _regDescriptions10, _regDescriptions11;
        private List<string> _regUnits00, _regUnits01, _regUnits10, _regUnits11;
        private bool _showAlarms = true;

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

        public OverviewViewModel()
        {
            XDocument doc = new XDocument();
            try
            {
                doc = XDocument.Load("config.xml");
                IsFileLoaded = Visibility.Visible;
            }
            catch
            {
                //GlobalCommands.ShowError.Execute(new Exception("Impossible to load XML configuration file."));
                IsFileLoaded = Visibility.Collapsed;
                return;
            }

            var p00 = doc.Root.Descendants("Plot00");            
            _regNames00 = Parser.ParseXmlElement(p00.Elements("reg_name").Nodes());
            _regDescriptions00 = GetRegDescription(_regNames00);
            _regUnits00 = GetRegUnits(_regNames00);

            var p01 = doc.Root.Descendants("Plot01");
            _regNames01 = Parser.ParseXmlElement(p01.Elements("reg_name").Nodes());
            _regDescriptions01 = GetRegDescription(_regNames01);
            _regUnits01 = GetRegUnits(_regNames01);

            var p10 = doc.Root.Descendants("Plot10");
            _regNames10 = Parser.ParseXmlElement(p10.Elements("reg_name").Nodes());
            _regDescriptions10 = GetRegDescription(_regNames10);
            _regUnits10 = GetRegUnits(_regNames10);

            var p11 = doc.Root.Descendants("Plot11");
            _regNames11 = Parser.ParseXmlElement(p11.Elements("reg_name").Nodes());
            _regDescriptions11 = GetRegDescription(_regNames11);
            _regUnits11 = GetRegUnits(_regNames11);

            // Create Plots
            Plot00 = CreatePlotModel(_regDescriptions00, _regUnits00);
            Plot01 = CreatePlotModel(_regDescriptions01, _regUnits01);
            Plot10 = CreatePlotModel(_regDescriptions10, _regUnits10);
            Plot11 = CreatePlotModel(_regDescriptions11, _regUnits11);

            // Subscribe to event(s)
            EventAggregator.OnAlarmMessageTransmitted += OnFlaggedAlarmMessageReceived;
            DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);
        }        

        private void DataRetrievedEventHandler(object sender, EventArgs e)
        {
            if (IsFileLoaded == Visibility.Visible)
            {
                AddSeries(Plot00, _regNames00);
                AddSeries(Plot01, _regNames01);
                AddSeries(Plot10, _regNames10);
                AddSeries(Plot11, _regNames11);
            }
        }

        private void AddSeries(PlotModel pM, IList<string> RegNames)
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

        private List<string> GetRegDescription(List<string> RegNames)
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
            }
            return RegDescriptions;
        }

        private List<string> GetRegUnits(List<string> RegNames)
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

        private PlotModel CreatePlotModel(List<string> RegDescriptions, List<string> RegUnits)
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
                Plotter.UnshowAnnotations(Plot00);
                Plotter.UnshowAnnotations(Plot01);
                Plotter.UnshowAnnotations(Plot10);
                Plotter.UnshowAnnotations(Plot11);
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
                Plotter.UnshowAnnotations(Plot00);
                Plotter.UnshowAnnotations(Plot01);
                Plotter.UnshowAnnotations(Plot10);
                Plotter.UnshowAnnotations(Plot11);
            }
            Plot00.InvalidatePlot(true);
            Plot01.InvalidatePlot(true);
            Plot10.InvalidatePlot(true);
            Plot11.InvalidatePlot(true);
        }
    }
}
