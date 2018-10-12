using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;
using TPIH.Gecco.WPF.Interfaces;
using TPIH.Gecco.WPF.Helpers;
using System.Windows.Input;
using System.IO;
using OfficeOpenXml;
using System.Threading;
using OxyPlot.Annotations;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class PulseGraphViewModel : ViewModelBase
    {
        private PlotModel _plot;
        private PlotModel _plotBool;
        private string _error, _status;
        private List<string> _dataFormat;
        private int _selectedDataFormat;
        private bool _isExportDataEnabled;
        private bool _isEventAlreadySubscribed;

        public ICommand ExportDataCommand { get; private set; }
        public List<string> DataFormat { get { return _dataFormat; } set { _dataFormat = value; OnPropertyChanged(() => DataFormat); } }
        public int SelectedDataFormat { get { return _selectedDataFormat; } set { _selectedDataFormat = value; OnPropertyChanged(() => SelectedDataFormat); } }
        public string Status { get { return _status; } set { _status = value; OnPropertyChanged(() => Status); } }

        public PlotModel Plot
        {
            get { return _plot; }
            set
            {
                _plot = value;
                OnPropertyChanged(() => Plot);
            }
        }
        public PlotModel PlotBool
        {
            get { return _plotBool; }
            set
            {
                _plotBool = value;
                OnPropertyChanged(() => PlotBool);
            }
        }
        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                OnPropertyChanged(() => Error);
            }
        }
               
        public bool IsExportDataEnabled { get { return _isExportDataEnabled; } set { _isExportDataEnabled = value; OnPropertyChanged(() => IsExportDataEnabled); } }
        private bool _isInternalChange = false;

        public PulseGraphViewModel()
        {
            Plot = new PlotModel("N3PR Data");
            Plot.Axes.Clear();
            var axis1 = new DateTimeAxis(AxisPosition.Bottom, "Time")
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
            };
            Plot.Axes.Add(axis1);
            Plot.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                Minimum = 0,
                Maximum = 50,
                IsZoomEnabled = true
            });
            Plot.Axes.Add(new LinearAxis(AxisPosition.Right, "%")
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

            PlotBool = new PlotModel("N3PR Logic Data");
            PlotBool.Axes.Clear();
            var axis2 = new DateTimeAxis(AxisPosition.Bottom, "Time")
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
            };
            PlotBool.Axes.Add(axis2);
            PlotBool.Axes.Add(new LinearAxis(AxisPosition.Left, "Data")
            {
                Key = "Primary",
                MajorGridlineStyle = LineStyle.Solid,
                Minimum = 0,
                Maximum = 2,
                MajorTickSize = 0.5,
                IsZoomEnabled = false                
            });           

            // Couple axis (boolean and standard)           
            axis1.AxisChanged += (s, e) =>
            {
                if (_isInternalChange)
                {
                    return;
                }

                _isInternalChange = true;
                axis2.Zoom(axis1.ActualMinimum, axis1.ActualMaximum);
                this.PlotBool.InvalidatePlot(false);
                _isInternalChange = false;
            };

            axis2.AxisChanged += (s, e) =>
            {
                if (_isInternalChange)
                {
                    return;
                }

                _isInternalChange = true;
                axis1.Zoom(axis2.ActualMinimum, axis2.ActualMaximum);
                this.Plot.InvalidatePlot(false);
                _isInternalChange = false;
            };

            // Initialize data-export fields
            DataFormat = new List<string> { "Csv", "Excel" };
            ExportDataCommand = new DelegateCommand(obj => ExportDataCommand_Execution(), obj => _isExportDataEnabled);

            // Subscribe to event(s)
            if (EventAggregator.OnMessageTransmitted == null)
                EventAggregator.OnMessageTransmitted += OnMessageReceived;
            if (!_isEventAlreadySubscribed)
            {
                DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(RefreshPlotsEventHandler);
                _isEventAlreadySubscribed = true;
            }
        }
    
        public void ShowPoints(IList<MeasurePoint> points)
        {
            if (points != null && points.Any() && points.All(p => p != null))
            {
                if (points[0].data_type == N3PR_Data.BOOL)
                {
                    Plotter.ShowPoints(points, PlotBool, Plotter.PRIMARY_AXIS);
                }
                else
                {
                    if (points[0].unit == "%")
                        Plotter.ShowPoints(points, Plot, Plotter.SECONDARY_AXIS);
                    else
                        Plotter.ShowPoints(points, Plot, Plotter.PRIMARY_AXIS);
                }

                // Now check the alarms
                if (DriverContainer.Driver.MbAlarm != null)
                {
                    List<string> alarmNames = DriverContainer.Driver.MbAlarm.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                    foreach (string name in alarmNames)
                    {
                        Plotter.AnnotateAlarms(
                            Plot,
                            DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList(),
                            N3PR_Data.ALARM_DESCRIPTION[N3PR_Data.ALARM_NAMES.IndexOf(name)]);
                        Plotter.AnnotateAlarms(
                            PlotBool,
                            DriverContainer.Driver.MbAlarm.Where(x => x.Reg_Name == name).ToList(),
                            "");
                    }
                }
            }
        }

        public void UnshowPoints(string name)
        {
            List<Series> tdbSerie = Plot.Series.Where(x => x.Title == name).ToList();
            if (tdbSerie.Count != 0)
            {
                foreach (var _tbd in tdbSerie)
                {
                    Plot.Series.Remove(_tbd);
                }
                Plot.InvalidatePlot(true); 
            }
            else
            {
                List<Series> tdboolSerie = PlotBool.Series.Where(x => x.Title == name).ToList();
                if (tdboolSerie != null)
                {
                    foreach (var _tbd in tdboolSerie)
                    {
                        PlotBool.Series.Remove(_tbd);
                    }
                }
                PlotBool.InvalidatePlot(true); 
            }
        }
        private void RefreshPlotsEventHandler(object sender, EventArgs e)
        {
            List<Series> tbrSeries = Plot.Series.ToList();
            List<Series> tbrSeriesBool = PlotBool.Series.ToList();
            // If there are some plots on the graph
            if (tbrSeries.Count != 0)
            {   
                foreach(Series tbrSerie in tbrSeries)
                {
                    var ls = (LineSeries)tbrSerie;
                    var myPoints = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == ls.Title).ToList();
                    AddPoints(ls, myPoints);                                                            
                }
                Plot.InvalidatePlot(true);
            }

            if (tbrSeriesBool.Count != 0)
            {
                foreach (Series tbrSerie in tbrSeriesBool)
                {
                    var ls = (LineSeries)tbrSerie;
                    var myPoints = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == ls.Title).ToList();
                    AddPoints(ls, myPoints);
                }
                PlotBool.InvalidatePlot(true);
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

        public void OnMessageReceived(ItemCheckedEvent e)
        {
            IList<MeasurePoint> myPoints = new List<MeasurePoint>();
            if (DriverContainer.Driver.MbData != null)
            {
                // The item was checked
                if (e.value)
                {
                    myPoints = DriverContainer.Driver.MbData.Where(x => x.Reg_Name == e.name).ToList();
                    ShowPoints(myPoints);
                }
                else // The item was unchecked
                {
                    // Unplot
                    UnshowPoints(e.name);
                }

                IsExportDataEnabled = (_plot.Series.Count() != 0) ? true : false;
            }
        }

        private void ExportDataCommand_Execution()
        {
            switch (_dataFormat[_selectedDataFormat])
            {
                case "Csv":
                    ExportCsv_Execution();
                    break;
                case "Excel":
                    ExportExcel_Execution();
                    break;
                default:
                    break;
            }
        }
        private void ExportExcel_Execution()
        {
            // Create SaveFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "N3PR_ExportData"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel Spreadsheet (.xlsx)|*.xlsx"; // Filter files by extension
            // Display 
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                Status = "Exporting....";
                string newFileWhere = dlg.FileName;

                try
                {
                    Thread exportDataThread = new Thread(tt => ExportExcel_Thread(newFileWhere));
                    exportDataThread.IsBackground = true;
                    exportDataThread.Start();
                }
                catch (Exception ex)
                {
                    GlobalCommands.ShowError.Execute(ex);
                }
            }
        }

        private void ExportExcel_Thread(string newFileWhere)
        {
            Stream fs = null;
            try
            {
                // Delete the file if it exists.
                if (File.Exists(newFileWhere))
                {
                    File.Delete(newFileWhere);
                }
                // Create the stream
                var fileInfo = new FileInfo(newFileWhere);
                using (ExcelPackage p = new ExcelPackage(fileInfo))
                {
                    ExcelWorkbook wb = p.Workbook;
                    ExcelWorksheet ws = wb.Worksheets.Add("N3PR-" + DateTime.Now.ToString("dd_MM_yyyy"));                    

                    // Get the data on the plot                    
                    int seriesOffset = 0;
                    for (int j = 0; j < _plot.Series.Count(); j++)
                    {
                        LineSeries _ls = (LineSeries)_plot.Series[j];
                        // Headers
                        ws.Cells[1, 1 + 2 * seriesOffset].Value = "Date";
                        ws.Cells[1, 2 + 2 * seriesOffset].Value = _ls.Title;                                                
                        for (int i = 0; i < _ls.Points.Count(); i++)
                        {
                            ws.Cells[i + 2, 1 + 2 * seriesOffset].Value = DateTime.FromOADate(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss");
                            ws.Cells[i + 2, 2 + 2 * seriesOffset].Value = _ls.Points[i].Y;
                        }
                        seriesOffset++;
                    }
                    for (int j = 0; j < _plotBool.Series.Count(); j++)
                    {
                        LineSeries _ls = (LineSeries)_plotBool.Series[j];
                        // Headers
                        ws.Cells[1, 1 + 2 * seriesOffset].Value = "Date";
                        ws.Cells[1, 2 + 2 * seriesOffset].Value = _ls.Title;
                        for (int i = 0; i < _ls.Points.Count(); i++)
                        {
                            ws.Cells[i + 2, 1 + 2 * seriesOffset].Value = DateTime.FromOADate(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss");
                            ws.Cells[i + 2, 2 + 2 * seriesOffset].Value = _ls.Points[i].Y;
                        }
                        seriesOffset++;
                    }
                    for (int j = 0; j < _plot.Annotations.Count(); j++)
                    {
                        LineAnnotation _as = (LineAnnotation)_plot.Annotations[j];
                        // Headers
                        ws.Cells[1, 1 + 2 * seriesOffset].Value = "Date";
                        ws.Cells[1, 2 + 2 * seriesOffset].Value = _as.Text;
                        ws.Cells[2, 1 + 2 * seriesOffset].Value = DateTime.FromOADate(_as.X).ToString("dd/MM/yyyy, HH: mm:ss");
                        ws.Cells[2, 2 + 2 * seriesOffset].Value = 1;
                        seriesOffset++; 
                    }

                    p.Save();
                    Status = "Done.";
                }
            }
            catch (Exception ex)
            {
                GlobalCommands.ShowError.Execute(ex);
                if (fs != null)
                    fs.Close();
            }
        }

        private void ExportCsv_Execution()
        {
            // Create SaveFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "N3PR_ExportData"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Text CSV (.csv)|*.csv"; // Filter files by extension
            // Display 
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                Status = "Exporting....";
                string newFileWhere = dlg.FileName;

                try
                {
                    Thread exportDataThread = new Thread(tt => ExportCsv_Thread(newFileWhere));
                    exportDataThread.IsBackground = true;
                    exportDataThread.Start();
                }
                catch (Exception ex)
                {
                    GlobalCommands.ShowError.Execute(ex);
                }
            }
        }

        private void ExportCsv_Thread(string newFileWhere)
        {
            StreamWriter fs = null;
            try
            {
                // Delete the file if it exists.
                if (File.Exists(newFileWhere))
                {
                    File.Delete(newFileWhere);
                }
                // Create the stream
                fs = new StreamWriter(newFileWhere);
                foreach (LineSeries _ls in _plot.Series)
                {
                    for (int i = 0; i < _ls.Points.Count(); i++)
                    {
                        var line = string.Format("{0},{1},{2}", DateTime.FromOADate(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss"), _ls.Title, _ls.Points[i].Y);
                        fs.WriteLine(line);
                        fs.Flush();                       
                    }
                }
                foreach (LineSeries _ls in _plotBool.Series)
                {
                    for (int i = 0; i < _ls.Points.Count(); i++)
                    {
                        var line = string.Format("{0},{1},{2}", DateTime.FromOADate(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss"), _ls.Title, _ls.Points[i].Y);
                        fs.WriteLine(line);
                        fs.Flush();
                    }
                }
                foreach (Annotation _as in _plot.Annotations)
                {
                    var _aas = (LineAnnotation)_as;
                    var line = string.Format("{0},{1},{2}", DateTime.FromOADate(_aas.X).ToString("dd/MM/yyyy, HH: mm:ss"), _aas.Text, (1).ToString());
                    fs.WriteLine(line);
                    fs.Flush();                    
                }

                fs.Close();
                Status = "Done.";
            }
            catch (Exception ex)
            {
                GlobalCommands.ShowError.Execute(ex);
                if (fs != null)
                    fs.Close();
            }
        }
    }
}