using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Timers;
using System.Windows;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class GeneratorErrorViewModel : ViewModelBase
    {
        private ObservableCollection<GeneratorErrorModel> _errors;
        public ObservableCollection<GeneratorErrorModel> Errors
        {
            get { return _errors; }
            set
            {
                _errors = value;
                OnPropertyChanged(() => Errors);
            }
        }

        public GeneratorErrorViewModel()
        {
            Errors = new ObservableCollection<GeneratorErrorModel>();
            DriverContainer.Driver.OnCommandReceived += Driver_OnCommandReceived;
        }


        private void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            if (e.Info == InfoEnum.GeneratorError)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Errors.Insert(0, new GeneratorErrorModel())));
            }
        }
    }
}
