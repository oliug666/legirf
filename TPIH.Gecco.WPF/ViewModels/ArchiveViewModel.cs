using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class ArchiveViewModel : ViewModelBase
    {
        private ObservableCollection<PulseMeasurement> _pulseMeasurements;
        public ObservableCollection<PulseMeasurement> PulseMeasurements
        {
            get { return _pulseMeasurements; }
            set
            {
                _pulseMeasurements = value;
                OnPropertyChanged(() => PulseMeasurements);
            }
        }

        private PulseMeasurement _selectedMeasurement;
        public PulseMeasurement SelectedMeasurement
        {
            get { return _selectedMeasurement; }
            set
            {
                _selectedMeasurement = value;
                if (value != null)
                {
                    PulseGraphViewModel.ShowPoints(value.Points);
                }
                OnPropertyChanged(() => SelectedMeasurement);
            }
        }

        private PulseGraphViewModel _pulseGraphViewModel;
        public PulseGraphViewModel PulseGraphViewModel
        {
            get { return _pulseGraphViewModel; }
            set
            {
                _pulseGraphViewModel = value;
                OnPropertyChanged(() => PulseGraphViewModel);
            }
        }

        #region ICommands

        public ICommand ClearCommand { get; private set; }

        

        #endregion

        public ArchiveViewModel()
        {
            PulseGraphViewModel = new PulseGraphViewModel();
            ClearCommand = new DelegateCommand(Clear);
            PulseMeasurements = new ObservableCollection<PulseMeasurement>();
            DriverContainer.Driver.OnCommandReceived += Driver_OnCommandReceived;
            var points = new List<MeasurePoint>();
            for (int i = 0; i < 23; i++)
            {
                points.Add(new MeasurePoint{Date = DateTime.Now, Reg_Name = "", i_val = (double)i/2});
            }
            PulseMeasurements.Add(new PulseMeasurement
            {
                Date = DateTime.Now,
                Points = points
            });
        }

        private void Clear(object obj)
        {
            PulseMeasurements.Clear();
        }

        private void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            if (e.Info == InfoEnum.PulseDisabled)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PulseMeasurements.Insert(0, new PulseMeasurement {Date = DateTime.Now, Points = e.MeasurePoints});
                    if (PulseMeasurements.Count > 100)
                    {
                        PulseMeasurements.RemoveAt(PulseMeasurements.Count-1);
                    }
                }));
            }
        }
    }
}
