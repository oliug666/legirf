using System;

namespace TPIH.Gecco.WPF.Helpers
{
    public class EventAggregator
    {
        public static void SignalItemChecked(string itemName, bool value)
        {
            OnCheckedItemTransmitted?.Invoke(new EventWithMessage(itemName, value));
        }

        public static void SignalShowUnshowAlarms(bool value)
        {
            OnAlarmMessageTransmitted?.Invoke(new EventWithMessage("", value));
        }

        public static void SignalIsRetrievingData(string message, bool value)
        {
            OnSignalIsRetrievingTransmitted?.Invoke(new EventWithMessage(message, value));
        }

        public static Action<EventWithMessage> OnCheckedItemTransmitted;
        public static Action<EventWithMessage> OnAlarmMessageTransmitted;
        public static Action<EventWithMessage> OnSignalIsRetrievingTransmitted;
    }
}
