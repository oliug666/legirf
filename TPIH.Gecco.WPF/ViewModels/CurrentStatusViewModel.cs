using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class CurrentStatusViewModel : ViewModelBase
    {
        private string _lastRefreshed;
        private Thread _loadLatestDataThread;
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());
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
                        GlobalCommands.ShowError.Execute(e);
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

        private void DataRetrievedEventHandler(object sender, System.EventArgs e)
        {
            if (DriverContainer.Driver.LatestData != null)
            {
                if (DriverContainer.Driver.LatestData.Count() != 0)
                {
                    // There is some shit
                    LastRefreshed = DriverContainer.Driver.LatestData[0].Date.ToString();
                    // Fill the TextBox
                    for (int i = 0; i < DriverContainer.Driver.LatestData.Count(); i++)
                    {
                        int idx = N3PR_Data.REG_NAMES.IndexOf(DriverContainer.Driver.LatestData[i].Reg_Name);
                        int div_factor = Convert.ToInt32(N3PR_Data.REG_DIVFACTORS[idx]);
                        LatestValues[idx] = (DriverContainer.Driver.LatestData[i].val).ToString();                                    
                    }
                }
                else
                {
                    // There is no shit
                    LastRefreshed = "No entries retrieved from database.";
                }
            }

            _loadLatestDataThread.Abort();
        }
    }
}
