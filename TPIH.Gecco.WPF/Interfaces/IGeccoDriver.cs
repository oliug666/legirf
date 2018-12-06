using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Interfaces
{
    public interface IGeccoDriver
    {        
        string LATEST { get; }
        string CUSTOM { get; }

        bool IsConnected { get; }
        event EventHandler OnDataRetrievalCompletedEventHandler;
        event Action<EventWithMessage> OnDataRetrievalEventHandler;
        event EventHandler OnLatestDataRetrievalCompleted;
        event EventHandler OnConnectionStatusChanged;

        IList<MeasurePoint> MbAlarm { get; }
        IList<MeasurePoint> MbData { get; }
        IList<MeasurePoint> LatestData { get; }

        void Connect(string ipa, int port, string dbname, string username, string password);

        void Disconnect();

        void GetDataFromLastXDays(string tableName, int lastDays);

        void GetLatestData(string tableName);
        void GetDataFromCalendarDays(string tableName, DateTime from, DateTime to);
    }
}
