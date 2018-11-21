using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using MySql.Data.MySqlClient;
using TPIH.Gecco.WPF.Interfaces;
using TPIH.Gecco.WPF.Models;
using System.Data;
using TPIH.Gecco.WPF.Settings;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Helpers;
using System.Windows;

namespace TPIH.Gecco.WPF.Drivers
{
    public class GeccoDriver : IGeccoDriver
    {
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());
        private readonly ResourceDictionary resourceDictionary = (ResourceDictionary)SharedResourceDictionary.SharedDictionary;

        private MySqlConnection _connection;

#if DEMO
        private Random rnd = new Random();
#endif

        private static IList<MeasurePoint> _mbData;
        private static IList<MeasurePoint> _mbAlarm;
        private static IList<MeasurePoint> _latestData;
        private Semaphore _isRetrieving = new Semaphore(1, 1);

#if !DEMO
        public bool IsConnected { get { return (_connection != null && (_connection.State == ConnectionState.Open)); } }
#else
        private bool _isConnected;
        public bool IsConnected { get { return _isConnected; } }
#endif
        public string Status;
        public event EventHandler OnDataRetrievalCompleted;
        public event EventHandler OnLatestDataRetrievalCompleted;
        public event EventHandler OnConnectionStatusChanged;

        public IList<MeasurePoint> MbData
        {
            get
            {
                if (IsConnected)
                {
                    return _mbData;
                }
                else
                {
                    return null;
                }
            }
            private set { _mbData = value; }
        }

        public IList<MeasurePoint> MbAlarm
        {
            get
            {
                if (IsConnected)
                {
                    return _mbAlarm;
                }
                else
                {
                    return null;
                }
            }
            private set { _mbAlarm = value; }
        }

        public IList<MeasurePoint> LatestData
        {
            get
            {
                if (IsConnected)
                {
                    return _latestData;
                }
                else
                {
                    return null;
                }
            }
            private set { _latestData = value; }
        }

        public GeccoDriver()
        {
        }

        private void Initialize(string ipAddress, int port, string dbname, string username, string password)
        {
#if !DEMO
            string connectionString = "SERVER=" + ipAddress + ";" + "PORT=" + port.ToString() + ";" + "DATABASE=" +
                dbname + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";";
            _connection = new MySqlConnection(connectionString);
#endif
            _mbData = new List<MeasurePoint>();
            _mbAlarm = new List<MeasurePoint>();
            _latestData = new List<MeasurePoint>();
        }

        public void Connect(string ipAddress, int port, string dbname, string username, string password)
        {
            Initialize(ipAddress, port, dbname, username, password);

            if (!IsConnected)
            {
#if !DEMO
                try
                {
                    _connection.Open();
                    _isRetrieving = new Semaphore(1, 1);
                }
                catch (MySqlException ex)
                {
                    //When handling errors, you can your application's response based 
                    //on the error number.
                    //The two most common error numbers when connecting are as follows:
                    //0: Cannot connect to server.
                    //1045: Invalid user name and/or password.
                    switch (ex.Number)
                    {
                        case 0:
                            Status = resourceDictionary["M_Error4"] + "";
                            break;

                        case 1045:
                            Status = resourceDictionary["M_Error5"] + "";
                            break;
                    }
                }
#else
                _isConnected = true;
#endif
            }

            OnConnectionStatusChanged?.Invoke(this, null);
        }

        //Close connection
        public void Disconnect()
        {
            if (IsConnected)
            {
#if !DEMO
                try
                {
                    _connection.Close();
                }
                catch (MySqlException ex)
                {
                    Status = ex.Message;
                }
#else
                _isConnected = false;
#endif
            }

            if (MbData != null)
                lock (MbData)
                    MbData.Clear();
            if (MbAlarm != null)
                lock (MbAlarm)
                    MbAlarm.Clear();
            if (LatestData != null)
                lock (LatestData)
                    LatestData.Clear();

            if (_isRetrieving != null)
                _isRetrieving.Close();

            EventAggregator.SignalIsRetrievingData(false);
            OnConnectionStatusChanged?.Invoke(this, null);
        }        

        public void GetLatestData(string tableName)
        {
            DateTime LatestDate = new DateTime();
            Thread.Sleep(1000);

            if (IsConnected)
            {
#if !DEMO
                _isRetrieving.WaitOne(); // Pause if there is someone already retrieving data
                EventAggregator.SignalIsRetrievingData(true);
                // First find the latest date            
                string dateQuery = "SELECT MAX(" + N3PR_DB.DATE + ") FROM " + tableName;
                try
                {
                    using (MySqlCommand msqlcommand = new MySqlCommand(dateQuery, _connection) { CommandTimeout = 60 })
                    {
                        var mdate = msqlcommand.ExecuteScalar();
                        LatestDate = ParseDate(mdate + "");
                    }                               
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + resourceDictionary["M_Error6"]));
                    DriverContainer.Driver.Disconnect();                    
                    return;
                }

                string selectQuery = "SELECT * FROM " + tableName + " WHERE " + tableName + "." + N3PR_DB.DATE
                    + " >= '" + LatestDate.AddSeconds(-10).ToString(N3PR_Data.DATA_FORMAT) + "'";
                try
                {
                    using (MySqlCommand msqlcommand = new MySqlCommand(selectQuery, _connection) { CommandTimeout = 60 })
                    {
                        using (MySqlDataReader msqldatareader = msqlcommand.ExecuteReader())
                        {
                            lock (LatestData)
                                LatestData = ParseDataReader(msqldatareader);
                        }
                    }                    
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(new Exception(e.Message + " - " + resourceDictionary["M_Error7"]));
                    DriverContainer.Driver.Disconnect();                    
                    return;
                }
            }
      
            if (!_isRetrieving.SafeWaitHandle.IsClosed)
                _isRetrieving.Release(1);

            EventAggregator.SignalIsRetrievingData(false);
#else
                LatestData = new List<MeasurePoint>();
                for (int i = 0; i < N3PR_Data.REG_NAMES.Count(); i++)
                {
                    LatestData.Add(new MeasurePoint
                    {
                        Date = DateTime.Now,
                        Reg_Name = N3PR_Data.REG_NAMES[i],
                        data_type = N3PR_Data.REG_TYPES[i],
                        unit = N3PR_Data.REG_MEASUNIT[i],
                        val = (N3PR_Data.REG_TYPES[i] == N3PR_Data.BOOL) ? rnd.Next(1) : rnd.Next(5000)
                    });
                }
                Thread.Sleep(1000);
            }
#endif
            // Fire the event            
            OnLatestDataRetrievalCompleted?.Invoke(this, null);           
        }

        public void GetDataFromLastXDays(string tableName, int lastDays)
        {
            // Get day today and calculate time interval
            DateTime right_now = DateTime.Now;
            string right_now_s = right_now.ToString(N3PR_Data.DATA_FORMAT);
            DateTime long_ago = DateTime.Now.AddDays(-lastDays);
            string long_ago_s = long_ago.ToString(N3PR_Data.DATA_FORMAT);

            // Create query
            string selectQuery = "SELECT * FROM " + tableName + " WHERE "+ N3PR_DB.DATE +
                " BETWEEN '" + long_ago_s + "' AND '" + right_now_s + "'";

            // Read
            if (!ExecuteQuery(selectQuery))
            {
                GlobalCommands.ShowError.Execute(new Exception(resourceDictionary["M_Error8"] + ""));
                DriverContainer.Driver.Disconnect();
                return;
            }            

            // Fire the event
            OnDataRetrievalCompleted?.Invoke(this, null);
        }

        public void GetDataFromCalendarDays(string tableName, DateTime From, DateTime To)
        {
            // Create query     
            string selectQuery = "SELECT * FROM " + tableName + " WHERE " + N3PR_DB.DATE +
                " BETWEEN '" + From.ToString(N3PR_Data.DATA_FORMAT) + "' AND '" + 
                To.AddHours(23).AddMinutes(59).AddSeconds(59).ToString(N3PR_Data.DATA_FORMAT) + "'";

            // Read
            if (!ExecuteQuery(selectQuery))            
            {
                GlobalCommands.ShowError.Execute(new Exception(resourceDictionary["M_Error9"] + ""));
                DriverContainer.Driver.Disconnect();
                return;
            }

            // Fire the event
            OnDataRetrievalCompleted?.Invoke(this, null);
        }

        private bool ExecuteQuery(string selectQuery)
        {
            List<MeasurePoint> _allData = new List<MeasurePoint>();
            // Read
            if (IsConnected)
            {
#if !DEMO
                try
                { 
                    _isRetrieving.WaitOne(); // Pause if there is someone already retrieving data
                    EventAggregator.SignalIsRetrievingData(true);

                    using (MySqlCommand msqlcmd = new MySqlCommand(selectQuery, _connection) { CommandTimeout = 60 })
                    {
                        using (MySqlDataReader msqldatareader = msqlcmd.ExecuteReader())
                        {
                            // Parse data reader
                            _allData = ParseDataReader(msqldatareader);
                            SortRetrievedData(_allData); // Sort query (data, alarms?)    
                        }
                    }

                    if (!_isRetrieving.SafeWaitHandle.IsClosed)
                        _isRetrieving.Release(1);

                    EventAggregator.SignalIsRetrievingData(false);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
#else
                SortRetrievedData(null);
                return true;
#endif
            }
            else
                return false;                                            
        }

        private List<MeasurePoint> ParseDataReader(IDataReader _dataReader)
        {
            int digits;
            List<MeasurePoint> _allData = new List<MeasurePoint>();
            while (_dataReader.Read())
            {
                int idxD = N3PR_Data.REG_NAMES.IndexOf(_dataReader[N3PR_DB.REG_NAME] + "");
                int idxA = N3PR_Data.ALARM_WARNING_NAMES.IndexOf(_dataReader[N3PR_DB.REG_NAME] + "");

                if (_dataReader[N3PR_DB.REG_NAME] + "" == "STE_IPS" || _dataReader[N3PR_DB.REG_NAME] + "" == "STE_OPS")
                    digits = 3;
                else
                    digits = 1;

                double value;
                if (idxD != -1)
                {
                    switch (N3PR_Data.REG_TYPES[idxD])
                    {
                        case N3PR_Data.INT:
                            value = Math.Round(Convert.ToInt32(_dataReader[N3PR_DB.IVAL] + "") / Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture), digits);
                            break;
                        case N3PR_Data.UINT:
                            value = Math.Round(Convert.ToUInt32(_dataReader[N3PR_DB.UIVAL] + "") / Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture), digits);
                            break;
                        case N3PR_Data.BOOL:
                            value = Convert.ToDouble(_dataReader[N3PR_DB.BVAL] + "");
                            break;
                        default:
                            value = Math.Round(Convert.ToInt32(_dataReader[N3PR_DB.IVAL] + "") / Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture), digits);
                            break;
                    }

                    if (N3PR_Data.REG_NAMES.Contains(_dataReader[N3PR_DB.REG_NAME] + ""))
                    {
                        _allData.Add(new MeasurePoint
                        {
                            Date = ParseDate(_dataReader[N3PR_DB.DATE] + ""),
                            Reg_Name = _dataReader[N3PR_DB.REG_NAME] + "",
                            val = value,
                            data_type = N3PR_Data.REG_TYPES[idxD],
                            unit = N3PR_Data.REG_MEASUNIT[idxD]
                        });
                    }
                }
                else if (idxA != -1)
                {
                    value = Convert.ToDouble(_dataReader[N3PR_DB.BVAL] + "");
                    if (N3PR_Data.ALARM_WARNING_NAMES.Contains(_dataReader[N3PR_DB.REG_NAME] + ""))
                    {
                        _allData.Add(new MeasurePoint
                        {
                            Date = ParseDate(_dataReader[N3PR_DB.DATE] + ""),
                            Reg_Name = _dataReader[N3PR_DB.REG_NAME] + "",
                            val = value,
                            data_type = N3PR_Data.BOOL,
                            unit = ""
                        });
                    }
                }            
            }
            return _allData;
        }

        private DateTime ParseDate(string sDate)
        {
            DateTime ParsedDate;
            try
            {
                ParsedDate = DateTime.ParseExact(sDate, N3PR_Data.DATA_FORMAT,
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                ParsedDate = DateTime.ParseExact(sDate, N3PR_Data.DATA_FORMAT,
                    System.Globalization.CultureInfo.CurrentCulture);
            }
            return ParsedDate;
        }               

        private void SortRetrievedData(List<MeasurePoint> aData)
        {
#if !DEMO
            // Sort the retrieved entries (alarms or data?)
            if (aData.Count() > 0)
            {
                // data
                lock (MbData)
                {
                    MbData.Clear();
                    foreach (MeasurePoint _mbp in aData)
                    {
                        if (N3PR_Data.REG_NAMES.Contains(_mbp.Reg_Name))
                            MbData.Add(_mbp);
                    }
                }
                lock (MbAlarm)
                {
                    MbAlarm.Clear();
                    // alarms
                    foreach (MeasurePoint _mbp in aData)
                    {
                        if (N3PR_Data.ALARM_WARNING_NAMES.Contains(_mbp.Reg_Name))
                            MbAlarm.Add(_mbp);
                    }
                }
            }
#else
            // Add 1000 datas
            lock (MbData)
            {
                MbData.Clear();
                for (int i = 0; i < 1000; i++)
                {
                    for (int j = 0; j < N3PR_Data.REG_NAMES.Count(); j++)
                    {
                        MbData.Add(new MeasurePoint
                        {
                            Date = DateTime.Now,
                            Reg_Name = N3PR_Data.REG_NAMES[j],
                            data_type = N3PR_Data.REG_TYPES[j],
                            unit = N3PR_Data.REG_MEASUNIT[j],
                            val = (N3PR_Data.REG_TYPES[j] == N3PR_Data.BOOL) ? rnd.Next(1) : rnd.Next(5000)
                        });
                    }
                }
            }
            Thread.Sleep(4000);
#endif
        }
    }    
}
