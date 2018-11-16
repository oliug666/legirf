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
