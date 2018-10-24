using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _versionNr, _connectionStatusColor;
        private bool _isErrorVisible;        

        public bool IsErrorVisible
        {
            get { return _isErrorVisible; }
            set
            {
                _isErrorVisible = value;
                OnPropertyChanged(() => IsErrorVisible);
            }
        }

        public string VersionNr { get { return _versionNr; } set { _versionNr = value; OnPropertyChanged(() => VersionNr); } }
        public string ConnectionStatusColor { get { return _connectionStatusColor; } set { _connectionStatusColor = value; OnPropertyChanged(() => ConnectionStatusColor); } }

        private Exception _error;
        public Exception Error
        {
            get { return _error; }
            set
            {
                _error = value;
                IsErrorVisible = value != null;
                OnPropertyChanged(() => Error);
            }
        }        

        private Visibility _isFileLoaded;
        public Visibility IsFileLoaded { get { return _isFileLoaded; } set { _isFileLoaded = value; OnPropertyChanged(() => IsFileLoaded); } }

        public void TerminateExecutionAndCloseConnections()
        {
            try
            {
                if (DriverContainer.Driver.IsConnected)
                {
                    DriverContainer.Driver.Disconnect();                    
                }
                DriverContainer.Driver.Dispose();
            }
            catch (Exception e)
            {
                GlobalCommands.ShowError.Execute(e);
            }
        }

        #region COmmands
        public ICommand CloseErrorCommand { get; private set; }

        public ICommand MinimizeCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand ToggleWindowStateCommand { get; private set; }

        #endregion

        public MainWindowViewModel()
        {
            // Display version
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            // Version increases each day, since 10/10/2018
            VersionNr = "Version: " + $"{version.Major}.{version.MajorRevision}.{version.Build}";

            CloseErrorCommand = new DelegateCommand(param =>
            {
                IsErrorVisible = false;
            });

            GlobalCommands.ShowError = new DelegateCommand(ShowError);

            IsErrorVisible = false;

            MinimizeCommand = new DelegateCommand(obj => { Application.Current.MainWindow.WindowState = WindowState.Minimized; });
            CloseCommand = new DelegateCommand(obj => Application.Current.MainWindow.Close());
            ToggleWindowStateCommand = new DelegateCommand(obj =>
            {
                Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState ==
                                                             WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            });

            ConnectionStatusColor = "Red";
            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(UpdateConnectionStatusIndicator);
        }

        private void UpdateConnectionStatusIndicator(object sender, EventArgs e)
        {
            if (DriverContainer.Driver.IsConnected)
                ConnectionStatusColor = "Green";
            else
                ConnectionStatusColor = "Red";
        }

        private void ShowError(object obj)
        {
            var err = obj as Exception;
            if (err != null)
            {
                Error = err;
            }
        }

        void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            /*
            if (e.Info == InfoEnum.PulseDisabled)
            {
                GraphViewModel.ShowPoints(e.MeasurePoints);
            }
            */
        }
    }
}
