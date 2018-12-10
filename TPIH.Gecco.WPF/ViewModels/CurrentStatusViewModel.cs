using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Models;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class CurrentStatusViewModel : ViewModelBase
    {        
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());

        private string _lastRefreshed;
        private Thread _loadLatestDataThread;
        private ObservableCollection<string> _latestValues;
        private bool _latestValuesEnabled;

        public ObservableCollection<string> LatestValues { get { return _latestValues; } set { _latestValues = value; OnPropertyChanged(() => LatestValues); } }
        public string LastRefreshed { get { return _lastRefreshed; } set { _lastRefreshed = value; OnPropertyChanged(() => LastRefreshed); } }
        public bool LatestValuesEnabled { get { return _latestValuesEnabled; } set { _latestValuesEnabled = value; OnPropertyChanged(() => LatestValuesEnabled); } }

        public CurrentStatusViewModel()
        {
            LastRefreshed = "";
            LatestValues = new ObservableCollection<string>();
            for (int i = 0; i < N3PR_Data.REG_NAMES.Count(); i++)
                LatestValues.Add("");

            Thread AutoRefreshThread = new Thread(AutoRefresh);
            AutoRefreshThread.IsBackground = true;
            AutoRefreshThread.Start();

            DriverContainer.Driver.OnLatestDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);
            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(ConnectionStatusChangedEventHandler);
        }

        private void AutoRefresh()
        {
            while (true)
            {
                if (DriverContainer.Driver.IsConnected)
                {
                    LatestValuesEnabled = true;

                    try
                    {
                        _loadLatestDataThread = new Thread(tt => DriverContainer.Driver.GetLatestData(_settings.TableName));
                        _loadLatestDataThread.IsBackground = true;
                        _loadLatestDataThread.Start();
                    }
                    catch (Exception e)
                    {
                        GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error10"]));
                    }
                    Thread.Sleep(30 * 1000);
                }
                else
                {
                    LatestValuesEnabled = false;
                    Thread.Sleep(200);
                }
            }
        }

        private void ConnectionStatusChangedEventHandler(object sender, System.EventArgs e)
        {
            if (!DriverContainer.Driver.IsConnected)
            {
                for (int i= 0;i<LatestValues.Count; i++)
                    LatestValues[i] = "";
                LastRefreshed = "";

                LatestValuesEnabled = false;
            }
        }

        private void DataRetrievedEventHandler(object sender, System.EventArgs e)
        {            
            // Let's make a local copy (for thread safety)
            IList<MeasurePoint> _latestData = DriverContainer.Driver.LatestData;
            LastRefreshed = SharedResourceDictionary.SharedDictionary["M_Error11"] + "";
            if (_latestData != null)
            {                
                if (_latestData.Count != 0)
                {
                    // There is some shit
                    LastRefreshed = _latestData[0].Date.ToString();
                    // Fill the TextBox
                    for (int i = 0; i < _latestData.Count(); i++)
                    {
                        int idx = N3PR_Data.REG_NAMES.IndexOf(_latestData[i].Reg_Name);
                        if (idx != -1 && LatestValues.Count > 0)
                        {
                            double div_factor = Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idx], CultureInfo.InvariantCulture);
                            LatestValues[idx] = (_latestData[i].val).ToString();
                        }
                    }
                }                
            }
        }
    }
}
