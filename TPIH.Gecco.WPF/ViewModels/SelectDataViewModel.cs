using System;
using System.Collections.Generic;
using System.Linq;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.ViewModels
{
    class SelectDataViewModel : ViewModelBase
    {
        private List<CheckedListItem> _availablePlottableObjects;
        private bool _enablePlottableObjects;
        public List<CheckedListItem> AvailablePlottableObjects { get { return _availablePlottableObjects; } set { _availablePlottableObjects = value; OnPropertyChanged(() => AvailablePlottableObjects); } }
        public bool EnablePlottableObjects { get { return _enablePlottableObjects; } set { _enablePlottableObjects = value; OnPropertyChanged(() => EnablePlottableObjects); } }

        public SelectDataViewModel()
        {
            AvailablePlottableObjects = new List<CheckedListItem>();
            for (int i = 0; i < N3PR_Data.REG_NAMES.Count(); i++)
            {
                AvailablePlottableObjects.Add(new CheckedListItem(i, N3PR_Data.REG_DESCRIPTION[i] + " [" + N3PR_Data.REG_NAMES[i] + "]",
                    N3PR_Data.REG_NAMES[i], N3PR_Data.REG_MEASUNIT[i], false, N3PR_Data.REG_TYPES[i]));
            }

            DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);
            DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(ConnectionStatusChangedEventHandler);
            EventAggregator.OnSignalIsRetrievingTransmitted += SignalIsRetrievingEventHandler;
        }

        private void DataRetrievedEventHandler(object sender, System.EventArgs e)
        {
            EnablePlottableObjects = false;
            // Lets make a local copy (thread safety)
            IList<MeasurePoint> _mbData = DriverContainer.Driver.MbData;
            if (_mbData != null)
            {                             
                if (_mbData.Count() != 0)
                    EnablePlottableObjects = true;
            }                            
        }

        private void ConnectionStatusChangedEventHandler(object sender, System.EventArgs e)
        {
            // Lets make a local copy (thread safety)
            IList<MeasurePoint> _mbData = DriverContainer.Driver.MbData;
            if (_mbData != null && DriverContainer.Driver.IsConnected)
            {
                if (_mbData.Count() != 0)
                    EnablePlottableObjects = true;
            }
            else
            {
                // Uncheck elements (if checked)
                foreach (CheckedListItem cli in AvailablePlottableObjects)
                {
                    cli.IsChecked = false;
                }
                EnablePlottableObjects = false;
            }            
        }

        private void SignalIsRetrievingEventHandler(ItemCheckedEvent e)
        {
            if (e.value)
                EnablePlottableObjects = false;
            else
                EnablePlottableObjects = true;
        }
    }
}
