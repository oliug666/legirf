using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TPIH.Gecco.WPF.Interfaces;

namespace TPIH.Gecco.WPF.Helpers
{
    public class EventAggregator
    {
        public static void Broadcast(string message, bool value)
        {
            OnMessageTransmitted?.Invoke(new ItemCheckedEvent(message, value));
        }

        public static Action<ItemCheckedEvent> OnMessageTransmitted;
    }
}
