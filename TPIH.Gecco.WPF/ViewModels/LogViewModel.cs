using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Settings;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        public string FileName
        {
            get { return _logger.FileName; }
            set
            {
                _logger.FileName = value;
                _settings.DefaultFileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public string Path
        {
            get { return _logger.Path; }
            set
            {
                _logger.Path = value;
                _settings.DefaultPath = value;
                OnPropertyChanged("Path");
            }
        }

        private bool _isLogEnabled;
        public bool IsLogEnabled
        {
            get { return _isLogEnabled; }
            set
            {
                _isLogEnabled = value && LogState;
                if (value)
                {
                    LinesWritten = 0;
                }
                OnPropertyChanged("IsLogEnabled");
            }
        }

        public bool LogState
        {
            get { return Directory.Exists(Path) && !string.IsNullOrEmpty(FileName); }
        }

        private long _linesWritten;
        public long LinesWritten
        {
            get { return _linesWritten; }
            set
            {
                _linesWritten = value;
                OnPropertyChanged("LinesWritten");
            }
        }

        #region Commands
        public ICommand BrowseCommand { get; private set; }
        #endregion


        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());        
        private readonly PulseLogger _logger;

        public LogViewModel()
        {
            _logger = new PulseLogger();
            FileName = _settings.DefaultFileName;
            Path = _settings.DefaultPath;

            BrowseCommand = new DelegateCommand(Browse);
        }

        private void Browse(object obj)
        {
            var param = obj as string;
            if (param != null)
            {
                switch (param)
                {
                    case "Path":
                        var pathDialog = new FolderBrowserDialog();
                        if (pathDialog.ShowDialog() == DialogResult.OK)
                        {
                            Path = pathDialog.SelectedPath;
                        }
                        break;
                    case "File":
                        var fileDialog  = new OpenFileDialog();
                        if (fileDialog.ShowDialog() == DialogResult.OK)
                        {
                            FileName = fileDialog.FileNames.First();
                        }
                        break;
                }
            }
        }
        
        void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            if (IsLogEnabled && e.Info == InfoEnum.PulseDisabled)
            {
                LinesWritten++;
                try
                {
                    //_logger.Write(_calculator.CalculateSummary(e.MeasurePoints, e.SetDuration));
                }
                catch (Exception exp)
                {
                    OnError(exp);
                    IsLogEnabled = false;
                }
            }
        }
    }
}
