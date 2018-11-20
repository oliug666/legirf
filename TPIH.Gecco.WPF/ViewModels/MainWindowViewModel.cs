using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;

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

        private Visibility _isTab1Visible, _isTab2Visible, _isTab3Visible;
        public Visibility IsTab1Visible { get { return _isTab1Visible; } set { _isTab1Visible = value; OnPropertyChanged(() => IsTab1Visible); } }
        public Visibility IsTab2Visible { get { return _isTab2Visible; } set { _isTab2Visible = value; OnPropertyChanged(() => IsTab2Visible); } }
        public Visibility IsTab3Visible { get { return _isTab3Visible; } set { _isTab3Visible = value; OnPropertyChanged(() => IsTab3Visible); } }

        private OverviewViewModel _overviewViewModel, _overview2ViewModel, _overview3ViewModel;
        public OverviewViewModel Overview1VM { get { return _overviewViewModel; } set { _overviewViewModel = value; OnPropertyChanged(() => Overview1VM); } }
        public OverviewViewModel Overview2VM { get { return _overview2ViewModel; } set { _overview2ViewModel = value; OnPropertyChanged(() => Overview2VM); } }
        public OverviewViewModel Overview3VM { get { return _overview3ViewModel; } set { _overview3ViewModel = value; OnPropertyChanged(() => Overview3VM); } }

        public void TerminateExecutionAndCloseConnections()
        {
            try
            {
                if (DriverContainer.Driver.IsConnected)
                {
                    DriverContainer.Driver.Disconnect();                    
                }
            }
            catch (Exception e)
            {
                GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + SharedResourceDictionary.SharedDictionary["M_Error15"]));
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
            VersionNr = $"{version.Major}.{version.MajorRevision}.{version.Build}";

            CloseErrorCommand = new DelegateCommand(param =>
            {
                IsErrorVisible = false;
            });

            // Overviews
            Overview1VM = new OverviewViewModel(new List<string>() { "Plot0", "Plot1", "Plot2", "Plot3"});
            Overview2VM = new OverviewViewModel(new List<string>() { "Plot4", "Plot5", "Plot6", "Plot7" });
            Overview3VM = new OverviewViewModel(new List<string>() { "Plot8", "Plot9", "Plot10", "Plot11" });
            IsTab1Visible = Overview1VM.IsFileLoaded;
            IsTab2Visible = Overview2VM.IsFileLoaded;
            IsTab3Visible = Overview3VM.IsFileLoaded;

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
