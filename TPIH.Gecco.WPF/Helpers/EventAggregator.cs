using System;

namespace TPIH.Gecco.WPF.Helpers
{
    public class EventAggregator
    {
        public static void SignalItemChecked(string message, bool value)
        {
            OnCheckedItemTransmitted?.Invoke(new ItemCheckedEvent(message, value));
        }

        public static void SignalShowUnshowAlarms(bool value)
        {
            OnAlarmMessageTransmitted?.Invoke(new ItemCheckedEvent("", value));
        }

        public static void SignalIsRetrievingData(bool value)
        {
            OnSignalIsRetrievingTransmitted?.Invoke(new ItemCheckedEvent("", value));
        }

        public static Action<ItemCheckedEvent> OnCheckedItemTransmitted;
        public static Action<ItemCheckedEvent> OnAlarmMessageTransmitted;
        public static Action<ItemCheckedEvent> OnSignalIsRetrievingTransmitted;
    }
}
