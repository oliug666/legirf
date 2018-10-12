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
        private string _r00, _r01, _r10, _r11;
        private string _t00, _t01, _t10, _t11;
        private bool _isEventAlreadySubscribed;

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
                doc = XDocument.Load("default_graph_config.xml");
                IsFileLoaded = Visibility.Visible;
            }
            catch
            {
                //GlobalCommands.ShowError.Execute(new Exception("Impossible to load XML configuration file."));
                IsFileLoaded = Visibility.Collapsed;
                return;
            }

            var p00 = doc.Root.Descendants("Plot00");
            _r00 = ParseXmlElement(p00.Elements("reg_name").Nodes());
            if (N3PR_Data.REG_NAMES.IndexOf(_r00) != -1)
                _t00 = N3PR_Data.REG_DESCRIPTION[N3PR_Data.REG_NAMES.IndexOf(_r00)];
            else
                _t00 = " ";

            var p01 = doc.Root.Descendants("Plot01");
            _r01 = ParseXmlElement(p01.Elements("reg_name").Nodes());
            if (N3PR_Data.REG_NAMES.IndexOf(_r01) != -1)
                _t01 = N3PR_Data.REG_DESCRIPTION[N3PR_Data.REG_NAMES.IndexOf(_r01)];
            else
                _t01 = " ";

            var p10 = doc.Root.Descendants("Plot10");
            _r10 = ParseXmlElement(p10.Elements("reg_name").Nodes());
            if (N3PR_Data.REG_NAMES.IndexOf(_r10) != -1)
                _t10 = N3PR_Data.REG_DESCRIPTION[N3PR_Data.REG_NAMES.IndexOf(_r10)];
            else
                _t10 = " ";

            var p11 = doc.Root.Descendants("Plot11");
            _r11 = ParseXmlElement(p11.Elements("reg_name").Nodes());
            if (N3PR_Data.REG_NAMES.IndexOf(_r11) != -1)
                _t11 = N3PR_Data.REG_DESCRIPTION[N3PR_Data.REG_NAMES.IndexOf(_r11)];
            else
                _t11 = " ";            

            Plot00 = new PlotModel(_t00);
            Plot00.Axes.Clear();            
            Plot00.Axes.Add(new DateTimeAxis(AxisPosition.Bottom, "Time")
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
            Plot00.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = false
            });

            //
            Plot01 = new PlotModel(_t01);
            Plot01.Axes.Clear();
            Plot01.Axes.Add(new DateTimeAxis(AxisPosition.Bottom, "Time")
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
            Plot01.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = false
            });

            //
            Plot10 = new PlotModel(_t10);
            Plot10.Axes.Clear();
            Plot10.Axes.Add(new DateTimeAxis(AxisPosition.Bottom, "Time")
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
            Plot10.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = false
            });

            //
            Plot11 = new PlotModel(_t11);
            Plot11.Axes.Clear();
            Plot11.Axes.Add(new DateTimeAxis(AxisPosition.Bottom, "Time")
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
            Plot11.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = false
            });

            // Subscribe to event (data retrieved)
            if (!_isEventAlreadySubscribed)
            {
                DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);
                _isEventAlreadySubscribed = true;
            }
        }

        private void DataRetrievedEventHandler(object sender, EventArgs e)
        {
            if (IsFileLoaded == Visibility.Visible)
            {
                if (_t00 != " ")
                {
                    Plot00.Series.Clear();
                    var myPoints00 = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == _r00).ToList();
                    ShowPoints(myPoints00, N3PR_Data.REG_TYPES[N3PR_Data.REG_NAMES.IndexOf(_r00)], Plot00);
                }

                if (_t01 != " ")
                {
                    Plot01.Series.Clear();
                    var myPoints01 = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == _r01).ToList();
                    ShowPoints(myPoints01, N3PR_Data.REG_TYPES[N3PR_Data.REG_NAMES.IndexOf(_r01)], Plot01);
                }

                if (_t10 != " ")
                {
                    Plot10.Series.Clear();
                    var myPoints10 = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == _r10).ToList();
                    ShowPoints(myPoints10, N3PR_Data.REG_TYPES[N3PR_Data.REG_NAMES.IndexOf(_r10)], Plot10);
                }

                if (_t11 != " ")
                {
                    Plot11.Series.Clear();
                    var myPoints11 = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == _r11).ToList();
                    ShowPoints(myPoints11, N3PR_Data.REG_TYPES[N3PR_Data.REG_NAMES.IndexOf(_r11)], Plot11);
                }
            }
        }

        public void ShowPoints(IList<MeasurePoint> points, string data_type, PlotModel pM)
        {
            if (IsFileLoaded == Visibility.Visible)
            {
                if (points != null && points.Any() && points.All(p => p != null))
                {
                    // Draw plot
                    Plotter.ShowPoints(points, pM, Plotter.PRIMARY_AXIS);
                    // Annotate Alarms
                    if (DriverContainer.Driver.MbAlarm != null)
                    {
                        List<string> alarmNames = DriverContainer.Driver.MbAlarm.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                        foreach (string name in alarmNames)
                        {
                            Plotter.AnnotateAlarms(
                                pM,
                                DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList(),
                                N3PR_Data.ALARM_DESCRIPTION[N3PR_Data.ALARM_NAMES.IndexOf(name)]);                            
                        }                        
                    }     
                }
            }
        }

        private void AddPoints(LineSeries ls, IList<MeasurePoint> myPoints)
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
        private string ParseXmlElement(IEnumerable<XNode> nodes)
        {
            List<string> myS = new List<string>();
            foreach (XNode xn in nodes)
                myS.Add(xn.ToString());

            if (myS.Count > 0)
                return myS[0];
            else
                return "";
        }

    }
}
