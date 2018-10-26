using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using TPIH.Gecco.WPF.Helpers;

namespace TPIH.Gecco.WPF.ViewModels
{
    class ActionButtonViewModel : ViewModelBase
    {
        private CheckedAlarmItem _alarmEnabled;
        private bool _isPidButtonEnabled;
        private List<string> _filename;
        private List<string> _path;
        private string _fullPath;

        public ICommand OpenPIDCommand { get; private set; }
        public CheckedAlarmItem AlarmsEnabled { get { return _alarmEnabled; } set { _alarmEnabled = value; OnPropertyChanged(() => AlarmsEnabled); } }
        public ActionButtonViewModel()
        {
            XDocument doc = new XDocument();
            try
            {
                doc = XDocument.Load("config.xml");
                _isPidButtonEnabled = true;
            }
            catch
            {
                _isPidButtonEnabled = false;                
            }

            var p00 = doc.Root.Descendants("PID_File");
            _filename = Parser.ParseXmlElement(p00.Elements("Filename").Nodes());
            _path = Parser.ParseXmlElement(p00.Elements("Path").Nodes());

            _fullPath = "";
            if (_path != null)
                _fullPath = (_path.Count() > 0) ? _path[0] + "\\" : "";
            if (_filename != null)
            {
                _fullPath += (_filename.Count() > 0) ? _filename[0] : "";
                if (File.Exists(_fullPath))
                    _isPidButtonEnabled = true;
            }
            OpenPIDCommand = new DelegateCommand(obj => OpenPIDCommand_Execution(), obj => _isPidButtonEnabled);
            AlarmsEnabled = new CheckedAlarmItem(true);
        }

        private void OpenPIDCommand_Execution()
        {
            if (File.Exists(_fullPath))
                System.Diagnostics.Process.Start(_fullPath);                         
        }        
    }
}
