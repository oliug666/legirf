using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPIH.Gecco.WPF.Helpers
{
    class CheckedAlarmItem
    {
        private bool _isChecked;
        public bool IsChecked { get { return _isChecked; } set { _isChecked = value; SendMessage(); } }

        public CheckedAlarmItem(bool ischecked)
        {
            IsChecked = ischecked;
        }

        private void SendMessage()
        {
            EventAggregator.SignalShowUnshowAlarms(this.IsChecked);
        }
    }
}
