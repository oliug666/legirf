using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using TPIH.Gecco.WPF.Annotations;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class SetPropertiesViewModel : ViewModelBase
    {
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());
        private string _ipaddress, _port, _dbname, _tablename, _username, _password;
        
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
        private System.Timers.Timer _refreshPortsTimer = new Timer(1000);

        public SetPropertiesViewModel()
        {
            ToggleConnectionCommand = new DelegateCommand(obj => ToggleConnection()); //, obj => !IsConnected);
            IsPropertiesEnabled = false;
            // Load last used settings
            _ipaddress = _settings.IPAddress;
            _port = _settings.Port;
            _dbname = _settings.DBname;
            _tablename = _settings.TableName;
            _username = _settings.Username;
            _password = _settings.Password;
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
                GlobalCommands.ShowError.Execute(e);
            }

            if (!DriverContainer.Driver.IsConnected)
            { 
                GlobalCommands.ShowError.Execute(new Exception("Failed to connect to server. Check network connection and server properties."));
            }
        }

        private void Disconnect()
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Close connection to DB?", "Closing Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    DriverContainer.Driver.Disconnect();
                    DriverContainer.Driver.Dispose();
                    IsPropertiesEnabled = DriverContainer.Driver.IsConnected;
                }
                catch (Exception e)
                {
                    IsPropertiesEnabled = false;
                    GlobalCommands.ShowError.Execute(e);
                }
            }            
        }
    }
}
