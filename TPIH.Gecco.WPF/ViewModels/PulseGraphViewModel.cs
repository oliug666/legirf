﻿using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;
using TPIH.Gecco.WPF.Helpers;
using System.Windows.Input;
using System.IO;
using OfficeOpenXml;
using System.Threading;
using OxyPlot.Annotations;
using System.Windows;

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
        private bool _showAlarms = true;

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
            Plot = new PlotModel(SharedResourceDictionary.SharedDictionary["L_Graph1"] + "")
            {
                LegendBackground = OxyColors.White,
                LegendBorder = OxyColors.Black                
            };            
            Plot.Axes.Clear(); 
            var axis1 = new DateTimeAxis(AxisPosition.Bottom, SharedResourceDictionary.SharedDictionary["L_TimeAxis"] + "")
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
            Plot.Axes.Add(new LinearAxis(AxisPosition.Left, SharedResourceDictionary.SharedDictionary["L_DataAxis"] + "")
            {
                Key = Plotter.PRIMARY_AXIS,
                MajorGridlineStyle = LineStyle.Solid,
                Minimum = 0,
                Maximum = 50,
                IsZoomEnabled = true
            });
            Plot.Axes.Add(new LinearAxis(AxisPosition.Right, N3PR_Data.PERCENTAGE)
            {
                Key = Plotter.SECONDARY_AXIS,
                MajorGridlineStyle = LineStyle.Solid,
                Minimum = 0,
                Maximum = 100,
                MajorGridlineColor = OxyColors.LightBlue,
                TicklineColor = OxyColors.LightBlue,
                TitleColor = OxyColors.Blue,
                TextColor = OxyColors.Blue,
                IsZoomEnabled = true
            });
            Plot.MouseDown += new EventHandler<OxyMouseEventArgs>((sender, e) => Plotter.OnMouseDown(sender, e, Plot)); 

            PlotBool = new PlotModel(SharedResourceDictionary.SharedDictionary["L_Graph2"] + "")
            {
                LegendBackground = OxyColors.White,
                LegendBorder = OxyColors.Black
            };
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
                Key = Plotter.PRIMARY_AXIS,
                MajorGridlineStyle = LineStyle.Solid,
                Minimum = 0,
                Maximum = 2,
                MajorTickSize = 0.5,
                IsZoomEnabled = false
            });
            PlotBool.MouseDown += new EventHandler<OxyMouseEventArgs>((sender, e) => Plotter.OnMouseDown(sender, e, PlotBool));

            // Couple axis (boolean and standard)           
            axis1.AxisChanged += (s, e) =>
            {
                if (_isInternalChange)
                {
                    return;
                }

                if (Plot.Series.Count == 0)
                    return; //tobetested

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

                if (PlotBool.Series.Count == 0)
                    return; //tobetested

                _isInternalChange = true;
                axis1.Zoom(axis2.ActualMinimum, axis2.ActualMaximum);
                this.Plot.InvalidatePlot(false);
                _isInternalChange = false;
            };

            // Initialize data-export fields
            DataFormat = new List<string> { "Excel", "Csv"};
            ExportDataCommand = new DelegateCommand(obj => ExportDataCommand_Execution(), obj => _isExportDataEnabled);

            // Subscribe to event(s)
            EventAggregator.OnCheckedItemTransmitted += OnCheckedItemMessageReceived;
            EventAggregator.OnAlarmMessageTransmitted += OnFlaggedAlarmMessageReceived;            
            DriverContainer.Driver.OnDataRetrievalCompletedEventHandler += new EventHandler(RefreshPlotsEventHandler);
            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(ConnectionStatusChangedEventHandler);
        }
       
        public void ShowPoints(IList<MeasurePoint> points)
        {
            // Let's make a local copy (thread safety)
            IList<MeasurePoint> _mbAlarms = DriverContainer.Driver.MbAlarm;

            if (points != null && points.Any() && points.All(p => p != null))
            {
                if (points[0].data_type == N3PR_Data.BOOL)
                {
                    Plotter.ShowPoints(points, PlotBool, Plotter.PRIMARY_AXIS);                    
                }
                else
                {
                    if (points[0].unit == N3PR_Data.PERCENTAGE)
                        Plotter.ShowPoints(points, Plot, Plotter.SECONDARY_AXIS);
                    else
                        Plotter.ShowPoints(points, Plot, Plotter.PRIMARY_AXIS);                    
                }
                // Now check the alarms
                if (_showAlarms)
                {
                    if (_mbAlarms != null)
                    {
                        // Check the alarms                        
                        List<string> alarmNames = _mbAlarms.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                        if (Plot.Series.ToList().Count > 0)
                            Plotter.ShowAnnotations(alarmNames, _mbAlarms, Plot, true);
                        if (PlotBool.Series.ToList().Count > 0)
                            Plotter.ShowAnnotations(alarmNames, _mbAlarms, PlotBool, false);
                    }
                } 
            }
        }

        public void UnshowPoints(string name)
        {
            Plotter.UnshowPoints(Plot, name);
            Plotter.UnshowPoints(PlotBool, name);            
            
            // Delete Annotations too, if there are no more traces
            if (Plot.Series.ToList().Count == 0)
            {
                Plotter.ClearAnnotations(Plot);
            }
            if (PlotBool.Series.ToList().Count == 0)
            {
                Plotter.ClearAnnotations(PlotBool);
            }

            Plot.InvalidatePlot(true);
            PlotBool.InvalidatePlot(true);
        }

        private void RefreshPlotsEventHandler(object sender, EventArgs e)
        {
            List<Series> tbrSeries = Plot.Series.ToList();
            List<Series> tbrSeriesBool = PlotBool.Series.ToList();
            // Lets make a local copy (thread safety)
            IList<MeasurePoint> _mbData = DriverContainer.Driver.MbData;
            IList<MeasurePoint> _mbAlarms = DriverContainer.Driver.MbAlarm;
            
            // If there are some plots on the graph
            if (tbrSeries.Count != 0)
            {
                Plotter.RefreshSeries(_mbData, tbrSeries);
                if (_showAlarms)
                    Plotter.RefreshAnnotations(_mbAlarms, Plot, true);
            }

            if (tbrSeriesBool.Count != 0)
            {
                Plotter.RefreshSeries(_mbData, tbrSeriesBool);                                
                if (_showAlarms)
                    Plotter.RefreshAnnotations(_mbAlarms, PlotBool, false);
            }          

            Plot.InvalidatePlot(true);
            PlotBool.InvalidatePlot(true);
        }        

        public void OnCheckedItemMessageReceived(EventWithMessage e)
        {
            // Lets make a local copy (thread safety)
            IList<MeasurePoint> _mbData = DriverContainer.Driver.MbData;
            IList<MeasurePoint> myPoints = new List<MeasurePoint>();
            if (_mbData != null)
            {
                // The item was checked
                if (e.value == 1)
                {
                    myPoints = _mbData.Where(x => x.Reg_Name == e.name).ToList();
                    ShowPoints(myPoints);
                    Plot.InvalidatePlot(true);
                    PlotBool.InvalidatePlot(true);
                }
                else // The item was unchecked
                {
                    // Unplot
                    UnshowPoints(e.name);
                }

                IsExportDataEnabled = ((_plot.Series.Count() + _plotBool.Series.Count()) != 0) ? true : false;
            }
        }

        private void ConnectionStatusChangedEventHandler(object sender, EventArgs e)
        {
            if (!DriverContainer.Driver.IsConnected)
            {
                Plotter.ClearPoints(Plot);
                Plotter.ClearPoints(PlotBool);
                Plotter.ClearAnnotations(Plot);
                Plotter.ClearAnnotations(PlotBool);
                Plot.InvalidatePlot(true);
                PlotBool.InvalidatePlot(true);
                IsExportDataEnabled = false;
            }
        }

        public void OnFlaggedAlarmMessageReceived(EventWithMessage e)
        {
            // Let's make a local copy (thread safety)
            IList<MeasurePoint> _mbAlarms = DriverContainer.Driver.MbAlarm;

            if (e.value == 1) // show annotations
            {
                _showAlarms = true;
                // Refresh Annotations
                if (Plot.Series.ToList().Count > 0)
                    Plotter.ClearAnnotations(Plot);
                if (PlotBool.Series.ToList().Count > 0)
                    Plotter.ClearAnnotations(PlotBool);
                if (_mbAlarms != null)
                {
                    List<string> alarmNames = _mbAlarms.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                    if (Plot.Series.ToList().Count > 0)
                        Plotter.ShowAnnotations(alarmNames, _mbAlarms, Plot, true);
                    if (PlotBool.Series.ToList().Count > 0)
                        Plotter.ShowAnnotations(alarmNames, _mbAlarms, PlotBool, false);
                }
            }
            else // unshow annotations
            {
                _showAlarms = false;
                Plotter.ClearAnnotations(Plot);
                Plotter.ClearAnnotations(PlotBool);
            }
            Plot.InvalidatePlot(true);
            PlotBool.InvalidatePlot(true);
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
                    GlobalCommands.ShowError.Execute(new Exception(ex.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error16"]));
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
                            ws.Cells[i + 2, 1 + 2 * seriesOffset].Value = DateTimeAxis.ToDateTime(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss");
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
                            ws.Cells[i + 2, 1 + 2 * seriesOffset].Value = DateTimeAxis.ToDateTime(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss");
                            ws.Cells[i + 2, 2 + 2 * seriesOffset].Value = _ls.Points[i].Y;
                        }
                        seriesOffset++;
                    }

                    if (_plot.Annotations.Count() != 0)
                    {
                        ws.Cells[1, 1 + 2 * seriesOffset].Value = "Alarm Date";
                        ws.Cells[1, 2 + 2 * seriesOffset].Value = "Alarm Value";
                        ws.Cells[1, 3 + 2 * seriesOffset].Value = "Alarm Description";
                        for (int j = 0; j < _plot.Annotations.Count(); j++)
                        {
                            TooltipAnnotation _as = (TooltipAnnotation)_plot.Annotations[j];
                            // Headers                        
                            ws.Cells[2 + j, 1 + 2 * seriesOffset].Value = DateTimeAxis.ToDateTime(_as.X).ToString("dd/MM/yyyy, HH: mm:ss");
                            if (_as.Color.Equals(OxyColors.Green))
                                ws.Cells[2 + j, 2 + 2 * seriesOffset].Value = 0;
                            else
                                ws.Cells[2 + j, 2 + 2 * seriesOffset].Value = 1;
                            ws.Cells[2 + j, 3 + 2 * seriesOffset].Value = _as.AuxText;
                        }
                    }
                    else
                    {
                        if (_plotBool.Annotations.Count() != 0)
                        {
                            ws.Cells[1, 1 + 2 * seriesOffset].Value = "Alarm Date";
                            ws.Cells[1, 2 + 2 * seriesOffset].Value = "Alarm Value";
                            ws.Cells[1, 3 + 2 * seriesOffset].Value = "Alarm Description";
                            for (int j = 0; j < _plotBool.Annotations.Count(); j++)
                            {
                                TooltipAnnotation _as = (TooltipAnnotation)_plotBool.Annotations[j];
                                // Headers                        
                                ws.Cells[2 + j, 1 + 2 * seriesOffset].Value = DateTimeAxis.ToDateTime(_as.X).ToString("dd/MM/yyyy, HH: mm:ss");
                                if (_as.Color.Equals(OxyColors.Green))
                                    ws.Cells[2 + j, 2 + 2 * seriesOffset].Value = 0;
                                else
                                    ws.Cells[2 + j, 2 + 2 * seriesOffset].Value = 1;
                                ws.Cells[2 + j, 3 + 2 * seriesOffset].Value = _as.AuxText;
                            }
                        }
                    }

                    seriesOffset++;

                    p.Save();
                    Status = "Done.";
                }
            }
            catch (Exception ex)
            {
                GlobalCommands.ShowError.Execute(new Exception(ex.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error17"]));
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
                    GlobalCommands.ShowError.Execute(new Exception(ex.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error18"]));
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
                        var line = string.Format("{0},{1},{2}", DateTimeAxis.ToDateTime(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss"), _ls.Title, _ls.Points[i].Y);
                        fs.WriteLine(line);
                        fs.Flush();                       
                    }
                }
                foreach (LineSeries _ls in _plotBool.Series)
                {
                    for (int i = 0; i < _ls.Points.Count(); i++)
                    {
                        var line = string.Format("{0},{1},{2}", DateTimeAxis.ToDateTime(_ls.Points[i].X).ToString("dd/MM/yyyy, HH: mm:ss"), _ls.Title, _ls.Points[i].Y);
                        fs.WriteLine(line);
                        fs.Flush();
                    }
                }

                if (_plot.Annotations.Count() != 0)
                {
                    foreach (Annotation _as in _plot.Annotations)
                    {
                        var _aas = (TooltipAnnotation)_as;
                        string line;
                        if (_aas.Color.Equals(OxyColors.Green))
                            line = string.Format("{0},{1},{2}", DateTimeAxis.ToDateTime(_aas.X).ToString("dd/MM/yyyy, HH: mm:ss"), _aas.AuxText, (0).ToString());
                        else
                            line = string.Format("{0},{1},{2}", DateTimeAxis.ToDateTime(_aas.X).ToString("dd/MM/yyyy, HH: mm:ss"), _aas.AuxText, (1).ToString());

                        fs.WriteLine(line);
                        fs.Flush();
                    }
                }
                else
                {
                    if (_plotBool.Annotations.Count() != 0)
                    {
                        foreach (Annotation _as in _plotBool.Annotations)
                        {
                            var _aas = (TooltipAnnotation)_as;
                            string line;
                            if (_aas.Color.Equals(OxyColors.Green))
                                line = string.Format("{0},{1},{2}", DateTimeAxis.ToDateTime(_aas.X).ToString("dd/MM/yyyy, HH: mm:ss"), _aas.AuxText, (0).ToString());
                            else
                                line = string.Format("{0},{1},{2}", DateTimeAxis.ToDateTime(_aas.X).ToString("dd/MM/yyyy, HH: mm:ss"), _aas.AuxText, (1).ToString());

                            fs.WriteLine(line);
                            fs.Flush();
                        }
                    }
                }

                fs.Close();
                Status = "Done.";
            }
            catch (Exception ex)
            {
                GlobalCommands.ShowError.Execute(new Exception(ex.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error19"]));
                if (fs != null)
                    fs.Close();
            }
        }                
    }
}