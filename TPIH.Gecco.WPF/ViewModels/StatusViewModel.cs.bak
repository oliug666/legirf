using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class StatusViewModel : ViewModelBase
    {
        private ObservableCollection<GeccoDriverArgs> _commands;

        public ObservableCollection<GeccoDriverArgs> Commands
        {
            get { return _commands; }
            set
            {
                _commands = value;
                OnPropertyChanged("Commands");
            }
        }
        public StatusViewModel()
        {
            Commands = new ObservableCollection<GeccoDriverArgs>();
        }

        void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    Commands.Insert(0, e);
                    if (Commands.Count > 10)
                    {
                        Commands.RemoveAt(Commands.Count - 1);
                    }

                }));
            }
        }
    }
}
