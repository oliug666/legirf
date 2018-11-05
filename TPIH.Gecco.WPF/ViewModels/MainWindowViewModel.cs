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

        private Visibility _isTab1Visible, _isTab2Visible;
        public Visibility IsTab1Visible { get { return _isTab1Visible; } set { _isTab1Visible = value; OnPropertyChanged(() => IsTab1Visible); } }
        public Visibility IsTab2Visible { get { return _isTab2Visible; } set { _isTab2Visible = value; OnPropertyChanged(() => IsTab1Visible); } }

        private OverviewViewModel _overviewViewModel;
        public OverviewViewModel OverviewVM { get { return _overviewViewModel; } set { _overviewViewModel = value; OnPropertyChanged(() => OverviewVM); } }

        private Overview2ViewModel _overview2ViewModel;
        public Overview2ViewModel Overview2VM { get { return _overview2ViewModel; } set { _overview2ViewModel = value; OnPropertyChanged(() => Overview2VM); } }

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
                GlobalCommands.ShowError.Execute(new Exception(e.Message + " - Error when trying to close application."));
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

            // Overviews
            OverviewVM = new OverviewViewModel();
            Overview2VM = new Overview2ViewModel();
            IsTab1Visible = OverviewVM.IsFileLoaded;
            IsTab2Visible = Overview2VM.IsFileLoaded;

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
    }
}
