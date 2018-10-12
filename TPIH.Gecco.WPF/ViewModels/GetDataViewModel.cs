using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    class GetDataViewModel : ViewModelBase
    {
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());
        private List<string> _timeIntervals;
        private int _selectedTimeInterval;

        private List<int> _lastDays;
        private string _status;
        private bool _isLoading;

        public List<string> TimeIntervals { get { return _timeIntervals; } set { _timeIntervals = value; OnPropertyChanged(() => TimeIntervals); } }
        public int SelectedTimeInterval { get { return _selectedTimeInterval; } set { _selectedTimeInterval = value; OnPropertyChanged(() => SelectedTimeInterval); } }
        public string Status { get { return _status; } set { _status = value; OnPropertyChanged(() => Status); } }
        public ICommand GetDataCommand { get; private set; }

        public GetDataViewModel()
        {
            GetDataCommand = new DelegateCommand(obj => GetDataCommand_Execution(), obj => (DriverContainer.Driver.IsConnected && !_isLoading));
            DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);

            _lastDays = new List<int> { 2, 7, 15, 30, 60, 90 };
            TimeIntervals = new List<string>();

            for (int i = 0; i < _lastDays.Count(); i++)
            {
                TimeIntervals.Add("Last " + _lastDays[i].ToString() + " days");
            }

            SelectedTimeInterval = 0;
        }      

        private void GetDataCommand_Execution()
        {
            if (DriverContainer.Driver.IsConnected)
            {
                Status = "Loading..."; _isLoading = true;
                try
                {
                    Thread loadDataThread = new Thread( tt => DriverContainer.Driver.GetDataFromLastXDays(_settings.TableName, _lastDays[SelectedTimeInterval]));
                    loadDataThread.IsBackground = true;
                    loadDataThread.Start();                    
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(e);
                }
            }
            else
            {
                GlobalCommands.ShowError.Execute(new Exception("Failed to retrieve data from server. Check network connection and try to re-connect."));
            }
        }

        private void DataRetrievedEventHandler(object sender, System.EventArgs e)
        {
            if (DriverContainer.Driver.MbData.Count() != 0)
            {
                // There is some shit
                Status = "Data Retrieved: " + DriverContainer.Driver.MbData.Count().ToString() + " entries.";
            }
            else
            {
                // There is no shit
                Status = "No entries retrieved from database.";
            }
            _isLoading = false;
        }
    }
}
