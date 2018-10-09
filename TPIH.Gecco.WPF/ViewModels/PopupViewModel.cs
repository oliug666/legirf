using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class PopupViewModel : ViewModelBase
    {
        private ViewModelBase _viewModelContent;

        public ViewModelBase ViewModelContent
        {
            get { return _viewModelContent; }
            set
            {
                _viewModelContent = value;
                OnPropertyChanged(() => ViewModelContent);
            }
        }
    }
}
