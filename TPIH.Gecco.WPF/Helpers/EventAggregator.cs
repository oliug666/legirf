using System;

namespace TPIH.Gecco.WPF.Helpers
{
    public class EventAggregator
    {
        public static void SignalItemChecked(string itemName, bool value)
        {
            OnCheckedItemTransmitted?.Invoke(new EventWithMessage(itemName, Convert.ToDouble(value)));
        }

        public static void SignalShowUnshowAlarms(bool value)
        {
            OnAlarmMessageTransmitted?.Invoke(new EventWithMessage("", Convert.ToDouble(value)));
        }

        public static void SignalIsRetrievingData(string message, bool value)
        {
            OnSignalIsRetrievingTransmitted?.Invoke(new EventWithMessage(message, Convert.ToDouble(value)));
        }

        public static Action<EventWithMessage> OnCheckedItemTransmitted;
        public static Action<EventWithMessage> OnAlarmMessageTransmitted;
        public static Action<EventWithMessage> OnSignalIsRetrievingTransmitted;
    }
}
