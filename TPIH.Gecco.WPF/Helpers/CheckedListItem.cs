using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.ViewModels;

namespace TPIH.Gecco.WPF.Helpers
{
    class CheckedListItem : ViewModelBase
    {
        //protected readonly IEventAggregator _eventAggregator;
        private bool _isChecked;

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get { return _isChecked; } set { _isChecked = value; OnPropertyChanged(() => IsChecked); SendMessage(); } }
        public string RegName { get; set; }
        public string Unit { get; set; }
        public string Data_Type { get; set; }

        public CheckedListItem(int id, string name, string regname, string unit, bool ischecked, string data_type)
        {
            Id = id;
            Name = name;
            IsChecked = ischecked;
            RegName = regname;
            Unit = unit;
            Data_Type = data_type;
        }

        private void SendMessage()
        {
            EventAggregator.SignalItemChecked(this.RegName, this.IsChecked);
        }
    }
}
