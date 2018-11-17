using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.ViewModels
{
    class ActionButtonViewModel : ViewModelBase
    {
        private CheckedAlarmItem _alarmEnabled;
        private bool _isPidButtonEnabled;
        private List<string> _filename;
        private List<string> _path;
        private string _fullPath, _alarmsActive, _warningsActive;

        public ICommand OpenPIDCommand { get; private set; }
        public CheckedAlarmItem AlarmsEnabled { get { return _alarmEnabled; } set { _alarmEnabled = value; OnPropertyChanged(() => AlarmsEnabled); } }
        public string AlarmsActive { get { return _alarmsActive; } set { _alarmsActive = value; OnPropertyChanged(() => AlarmsActive); } }
        public string WarningsActive { get { return _warningsActive; } set { _warningsActive = value; OnPropertyChanged(() => WarningsActive); } }

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

            AlarmsActive = "Gray";
            WarningsActive = "Gray";

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

            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(ConnectionStatusChangedEventHandler);
            DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);
        }

        private void DataRetrievedEventHandler(object sender, EventArgs e)
        {
            List<string> alarmNames = new List<string>();
            List<string> warningNames = new List<string>();
            List<bool> activeAlarmsFlags = new List<bool>();
            List<bool> activeWarningFlags = new List<bool>();

            // Let's make a local copy (thread safety)
            IList<MeasurePoint> _mbAlarms = DriverContainer.Driver.MbAlarm;

            if (_mbAlarms != null)
            {
                // Distinct Values
                List<string> uniqueRegNames = _mbAlarms.Select(x => x.Reg_Name).ToList().Distinct().ToList();
                foreach (string sg in uniqueRegNames)
                {
                    if (N3PR_Data.ALARM_NAMES.Contains(sg))
                        alarmNames.Add(sg);
                    else
                        warningNames.Add(sg);
                }
                activeAlarmsFlags = Plotter.AreThereActiveAlarms(alarmNames, _mbAlarms);
                activeWarningFlags = Plotter.AreThereActiveAlarms(warningNames, _mbAlarms);
            }

            AlarmsActive = "Green";
            if (activeAlarmsFlags != null)
                if (activeAlarmsFlags.Contains(true))
                    AlarmsActive = "Red";

            WarningsActive = "Green";
            if (activeWarningFlags != null)
                if (activeWarningFlags.Contains(true))
                    WarningsActive = "Orange";
        }

        private void ConnectionStatusChangedEventHandler(object sender, EventArgs e)
        {
            if (!DriverContainer.Driver.IsConnected)
            {
                AlarmsActive = "Gray";
                WarningsActive = "Gray";
            }
        }

        private void OpenPIDCommand_Execution()
        {
            if (File.Exists(_fullPath))
                System.Diagnostics.Process.Start(_fullPath);                         
        }        
    }
}
