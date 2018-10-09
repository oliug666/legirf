using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Interfaces
{
    public interface IGeccoDriver
    {
        bool IsConnected { get; }
        event EventHandler OnDataRetrievalCompleted;
        event EventHandler OnLatestDataRetrievalCompleted;

        IList<MeasurePoint> MbAlarm { get; }
        IList<MeasurePoint> MbData { get; }
        IList<MeasurePoint> LatestData { get; }

        void Connect(string ipa, int port, string dbname, string username, string password);

        void Disconnect();
        void Dispose();

        void GetDataFromLastXDays(string tableName, int lastDays);

        void GetLatestData(string tableName);
    }
}
