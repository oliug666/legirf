using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Models;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    class GetDataViewModel : ViewModelBase
    {
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());

        private List<int> _timeIntervals;
        private int _selectedTimeInterval;
        private Visibility _isProgressBarVisible;
        private string _status;
        private bool _isCalendarEnabled, _getDataCanExecute;
        private DateTime _to, _from;
        private bool _isAutoGetDataEnabled;
        private int _autoGetDataRefreshTime;
        private double _progressBarValue;
        private bool _hasGetDataBeenExecuteOnce = false;

        public bool GetDataIsEnabled { get { return _getDataCanExecute; } set { _getDataCanExecute = value; OnPropertyChanged(() => GetDataIsEnabled); } }
        public double ProgressBarValue { get { return _progressBarValue; } set { _progressBarValue = value; OnPropertyChanged(() => ProgressBarValue); } }
        public Visibility IsProgressBarVisible { get { return _isProgressBarVisible; } set { _isProgressBarVisible = value; OnPropertyChanged(() => IsProgressBarVisible); } }
        public List<int> TimeIntervals { get { return _timeIntervals; } set { _timeIntervals = value; OnPropertyChanged(() => TimeIntervals); } }
        public int SelectedTimeInterval
        {
            get { return _selectedTimeInterval; }
            set
            {
                _selectedTimeInterval = value;
                if (_timeIntervals[_selectedTimeInterval] == -1)
                    IsCalendarEnabled = true;
                else
                    IsCalendarEnabled = false;
                OnPropertyChanged(() => SelectedTimeInterval);
            }
        }
        public string Status { get { return _status; } set { _status = value; OnPropertyChanged(() => Status); } }
        public bool IsCalendarEnabled { get { return _isCalendarEnabled; } set { _isCalendarEnabled = value; OnPropertyChanged(() => IsCalendarEnabled); } }
        public DateTime To { get { return _to; } set { _to = value; OnPropertyChanged(() => To); } }
        public DateTime From { get { return _from; } set { _from = value; OnPropertyChanged(() => From); } }

        public ICommand GetDataCommand { get; private set; }

        public GetDataViewModel()
        {
            GetDataCommand = new DelegateCommand(obj => GetDataCommand_Execution()); 
            DriverContainer.Driver.OnDataRetrievalCompletedEventHandler += new EventHandler(DataRetrievalCompletedEventHandler);
            DriverContainer.Driver.OnDataRetrievalEventHandler += DataRetrievedEventHandler;
            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(OnConnectionStatusChangedEventHandler);

            TimeIntervals = new List<int>() { 2, 7, 15, 30, 60, -1}; // -1 = Custom

            To = DateTime.Today;
            From = DateTime.Today.AddDays(-1);
            IsCalendarEnabled = false;
            IsProgressBarVisible = Visibility.Hidden;

            SelectedTimeInterval = 0;

            // Auto get-data
            XDocument doc = new XDocument();
            try
            {
                doc = XDocument.Load("config.xml");
                var p00 = doc.Root.Descendants("Auto_GetData");
                _isAutoGetDataEnabled = (Parser.ParseXmlElement(p00.Elements("Enabled").Nodes()).First() == "1") ? true : false;
                _autoGetDataRefreshTime = Convert.ToInt32(Parser.ParseXmlElement(p00.Elements("Time_Period_Min").Nodes()).First());
            }
            catch
            {
                _isAutoGetDataEnabled = false;
            }         
            
            if (_isAutoGetDataEnabled)
            {
                Thread AutoGetDataThread = new Thread(tt => AutoGetDataThread_Execution(_autoGetDataRefreshTime));
                AutoGetDataThread.IsBackground = true;
                AutoGetDataThread.Start();
            }       
        }

        private void DataRetrievedEventHandler(EventWithMessage e)
        {
            ProgressBarValue = e.value;
        }

        private void OnConnectionStatusChangedEventHandler(object sender, EventArgs e)
        {
            if (!DriverContainer.Driver.IsConnected)
            {
                GetDataIsEnabled = false;
                IsCalendarEnabled = false;
                IsProgressBarVisible = Visibility.Hidden;
                Status = "";
            }
            else
            {
                GetDataIsEnabled = true;
                if (_timeIntervals[_selectedTimeInterval] == -1)
                    IsCalendarEnabled = true;
                else
                    IsCalendarEnabled = false;
            }
        }

        private void AutoGetDataThread_Execution(int interval_min)
        {
            while(true)
            {
                Thread.Sleep(1000 * 60 * interval_min);
                if (DriverContainer.Driver.IsConnected && GetDataIsEnabled && _hasGetDataBeenExecuteOnce)
                {
                    //
                    GetDataCommand_Execution();
                }
            }
        }

        private void GetDataCommand_Execution()
        {            
            _hasGetDataBeenExecuteOnce = true;
            if (DriverContainer.Driver.IsConnected && GetDataIsEnabled)
            {
                Status = "Loading...";
                IsProgressBarVisible = Visibility.Visible;
                ProgressBarValue = 0;
                GetDataIsEnabled = false;
                if (!_isCalendarEnabled)
                {
                    try
                    {
                        Thread loadDataThread = new Thread(tt => DriverContainer.Driver.GetDataFromLastXDays(_settings.TableName, _timeIntervals[_selectedTimeInterval]));
                        loadDataThread.IsBackground = true;
                        loadDataThread.Start();
                    }
                    catch (Exception e)
                    {
                        GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error12"]));
                    }
                }
                else
                {
                    try
                    {
                        Thread loadDataThread = new Thread(tt => DriverContainer.Driver.GetDataFromCalendarDays(_settings.TableName, _from, _to));
                        loadDataThread.IsBackground = true;
                        loadDataThread.Start();
                    }
                    catch (Exception e)
                    {
                        GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error13"]));
                    }
                }
            }
            else
            {
                GlobalCommands.ShowError.Execute(new Exception(SharedResourceDictionary.SharedDictionary["M_Error14"] + ""));
            }
        }

        private void DataRetrievalCompletedEventHandler(object sender, System.EventArgs e)
        {
            // Lets make a local copy (thread safety)
            IList<MeasurePoint> _mbData = DriverContainer.Driver.MbData;
            if (_mbData != null)
            {
                if (_mbData.Count() != 0)
                {
                    // There is some shit
                    Status = SharedResourceDictionary.SharedDictionary["L_DataRetrieved"] + ": " + _mbData.Count().ToString() + " " + SharedResourceDictionary.SharedDictionary["L_Entries"] + ".";
                }
                else
                {
                    // There is no shit
                    Status = SharedResourceDictionary.SharedDictionary["M_Error11"] + "";
                }
            }
            GetDataIsEnabled = true;
            IsProgressBarVisible = Visibility.Hidden;
        }        
    }
}
