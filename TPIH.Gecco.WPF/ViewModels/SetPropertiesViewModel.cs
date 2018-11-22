using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class SetPropertiesViewModel : ViewModelBase
    {
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());
        private string _ipaddress, _port, _dbname, _tablename, _username, _password;

        private bool _isConnectionToggleButtonEnabled;
        public bool IsConnectionToggleButtonEnabled { get { return _isConnectionToggleButtonEnabled; } set { _isConnectionToggleButtonEnabled = value; OnPropertyChanged(() => IsConnectionToggleButtonEnabled); } }
        /* To be activated on language change implementation
        private List<string> _languageList;
        public List<string> LanguageList { get { return _languageList; } set { _languageList = value; OnPropertyChanged(() => LanguageList); } }
        private int _selectedLanguage;
        public int SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                ChangeLanguage(_languageList[_selectedLanguage]);
                OnPropertyChanged(() => SelectedLanguage);
            }
        }
        */

        public string IPAddress
        {
            get { return _ipaddress; }
            set
            {
                _ipaddress = value;
                _settings.IPAddress = value;
                OnPropertyChanged(() => IPAddress);
                ToggleConnectionCommand.CanExecute(null);
            }
        }

        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                _settings.Port = value;
                OnPropertyChanged(() => Port);
                ToggleConnectionCommand.CanExecute(null);
            }
        }
        public string DBname
        {
            get { return _dbname; }
            set
            {
                _dbname = value;
                _settings.DBname = value;
                OnPropertyChanged(() => DBname);
                ToggleConnectionCommand.CanExecute(null);
            }
        }
        public string TableName
        {
            get { return _tablename; }
            set
            {
                _tablename = value;
                _settings.TableName = value;
                OnPropertyChanged(() => TableName);
                ToggleConnectionCommand.CanExecute(null);
            }
        }
        public string Username
        {
            get { return _username; }
            set
            {
                _settings.Username = value;
                _username = value;
                OnPropertyChanged(() => Username);
            }
        }    

        public string Password
        {
            get { return _password; }
            set
            {
                _settings.Password = value;
                _password = value;
                OnPropertyChanged(() => Password);
            }
        }

        private bool _isPropertiesEnabled;

        public bool IsPropertiesEnabled
        {
            get { return _isPropertiesEnabled; }
            set
            {
                _isPropertiesEnabled = value;
                OnPropertyChanged(() => IsPropertiesEnabled);
            }
        }                             

        #region Commands
        public ICommand ToggleConnectionCommand { get; private set; }

        public bool IsConnected
        {
            get { return DriverContainer.Driver.IsConnected; }
        }

        #endregion

        // Quick fix to update the ports available and also the Connect button state.
        private Timer _refreshPortsTimer = new Timer(1000);

        public SetPropertiesViewModel()
        {
            ToggleConnectionCommand = new DelegateCommand(obj => ToggleConnection());
            IsPropertiesEnabled = false;
            // Load last used settings
            _ipaddress = _settings.IPAddress;
            _port = _settings.Port;
            _dbname = _settings.DBname;
            _tablename = _settings.TableName;
            _username = _settings.Username;
            _password = _settings.Password;

            IsConnectionToggleButtonEnabled = true;

            /* Language change
            LanguageList = new List<string>
            {
                SharedResourceDictionary.SharedDictionary["L_English"] + "",
                SharedResourceDictionary.SharedDictionary["L_Italian"] + ""
            };
            SelectedLanguage = 0;
            */

            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(ConnectionStatusChangedEventHandler);
            EventAggregator.OnSignalIsRetrievingTransmitted += SignalIsRetrievingEventHandler;
        }

        private void SignalIsRetrievingEventHandler(EventWithMessage e)
        {
            // We block disconnection when retrieving data
            if (e.value)
                IsConnectionToggleButtonEnabled = false;
            else
                IsConnectionToggleButtonEnabled = true;
        }

        private void ConnectionStatusChangedEventHandler(object sender, EventArgs e)
        {
            // Refresh connection status
            OnPropertyChanged(() => IsConnected);
            IsPropertiesEnabled = DriverContainer.Driver.IsConnected;            
        }

        private void ToggleConnection()
        {
            if (IsConnected)
            {

                Disconnect();
            }
            else
            {
                Connect();
            }
            OnPropertyChanged(()=> IsConnected);
        }

        private void Connect()
        {
            IsPropertiesEnabled = DriverContainer.Driver.IsConnected;

            try
            {
                DriverContainer.Driver.Disconnect();
                DriverContainer.Driver.Connect(IPAddress, Convert.ToInt16(Port), DBname, Username, Password);
                IsPropertiesEnabled = DriverContainer.Driver.IsConnected;
            }
            catch (Exception e)
            {
                IsPropertiesEnabled = false;
                OnPropertyChanged(() => IsConnected);
                GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error3"]));
            }

            if (!DriverContainer.Driver.IsConnected)
            { 
                GlobalCommands.ShowError.Execute(new Exception(SharedResourceDictionary.SharedDictionary["M_Error2"] + ""));
            }
        }

        private void Disconnect()
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(SharedResourceDictionary.SharedDictionary["D_Closing"]+"",
                SharedResourceDictionary.SharedDictionary["D_H_Closing"] + "", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    DriverContainer.Driver.Disconnect();
                    IsPropertiesEnabled = DriverContainer.Driver.IsConnected;
                }
                catch (Exception e)
                {
                    IsPropertiesEnabled = false;
                    GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error1"]));
                }
            }            
        }

        private void ChangeLanguage(string val)
        {
            var resources = new ResourceDictionary();
            if (val == SharedResourceDictionary.AvailableLanguages[0])
            {
                resources.Source = new Uri("..\\Resources\\Resources.en-US.xaml", System.UriKind.Relative);
                SharedResourceDictionary.ChangeLanguage(SharedResourceDictionary.dictionary_EN);
            }
            else if (val == SharedResourceDictionary.AvailableLanguages[1])
            {
                resources.Source = new Uri("..\\Resources\\Resources.it-IT.xaml", System.UriKind.Relative);
                SharedResourceDictionary.ChangeLanguage(SharedResourceDictionary.dictionary_IT);
            }

            var current = Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                    m => m.Source.OriginalString.Contains("Resources"));
            if (current != null)
                Application.Current.Resources.MergedDictionaries.Remove(current);
            Application.Current.Resources.MergedDictionaries.Add(resources);
        }
    }
}
