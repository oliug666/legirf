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
        public event EventHandler OnDataRetrievalCompletedEventHandler;
        public event Action<EventWithMessage> OnDataRetrievalEventHandler;
        public event EventHandler OnLatestDataRetrievalCompleted;
        public event EventHandler OnConnectionStatusChanged;

        public string LATEST { get { return "latest"; } }
        public string CUSTOM { get { return "custom"; } }

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

            EventAggregator.SignalIsRetrievingData("", false);
            OnConnectionStatusChanged?.Invoke(this, null);
        }        

        public void GetLatestData(string tableName)
        {
            DateTime LatestDate = new DateTime();
            Thread.Sleep(1000);

            if (IsConnected)
            {
#if !DEMO
                if (!_isRetrieving.WaitOne(5000)) // Pause if there is someone already retrieving data (after 5 secs, kill it)
                     return;

                EventAggregator.SignalIsRetrievingData(LATEST, true);
                // First find the latest date     
                string dateQuery = "SELECT " + N3PR_DB.DATE + " FROM " + tableName + " ORDER BY " + N3PR_DB.ID + " DESC LIMIT 1";
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

                string selectQuery = "SELECT " + N3PR_DB.DATE + ", " + N3PR_DB.REG_NAME + ", " + N3PR_DB.IVAL
                    + " FROM ( SELECT * FROM "+tableName+" ORDER BY "+ N3PR_DB.ID + " DESC LIMIT 2000 ) as top_subset " +
                    " WHERE top_subset." + N3PR_DB.DATE + " >= '" + LatestDate.AddSeconds(-10).ToString(N3PR_Data.DATA_FORMAT) + "'";
                try
                {
                    using (MySqlCommand msqlcommand = new MySqlCommand(selectQuery, _connection) { CommandTimeout = 60 })
                    {
                        msqlcommand.CommandTimeout = 1000;
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

            EventAggregator.SignalIsRetrievingData(LATEST, false);
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
            lock (MbData)
                MbData.Clear();
            lock (MbAlarm)
                MbAlarm.Clear();
                    
            // Read
            if (!ExecuteQuery(DateTime.Now.AddDays(-lastDays), DateTime.Now, tableName))
            {
                GlobalCommands.ShowError.Execute(new Exception(resourceDictionary["M_Error8"] + ""));
                DriverContainer.Driver.Disconnect();
                return;
            }                                 

            // Fire the event
            OnDataRetrievalCompletedEventHandler?.Invoke(this, null);

            // --SPLIT QUERY--
            // If the interval is bigger than two days, split the queries 
            //int splitDays = 10;           
            //if ((right_now.Subtract(long_ago)).Days > splitDays)
            //{
            //    if (!SplitQuery(long_ago, right_now, tableName, splitDays))
            //    {
            //        GlobalCommands.ShowError.Execute(new Exception(resourceDictionary["M_Error8"] + ""));
            //        DriverContainer.Driver.Disconnect();
            //        return;
            //    }
            //}
            //else
            //{
            //    // Create query
            //    if (!ExecuteQuery(long_ago, right_now, tableName))
            //    {
            //        GlobalCommands.ShowError.Execute(new Exception(resourceDictionary["M_Error8"] + ""));
            //        DriverContainer.Driver.Disconnect();
            //        return;
            //    }                
            //    OnDataRetrievalEventHandler?.Invoke(new EventWithMessage("", 100));
            //}
            //OnDataRetrievalEventHandler?.Invoke(new EventWithMessage("", 100));          
        }

        public void GetDataFromCalendarDays(string tableName, DateTime From, DateTime To)
        {
            lock (MbData)
                MbData.Clear();
            lock (MbAlarm)
                MbAlarm.Clear();

            // Read
            if (!ExecuteQuery(From, To.AddHours(23).AddMinutes(59).AddSeconds(59), tableName))            
            {
                GlobalCommands.ShowError.Execute(new Exception(resourceDictionary["M_Error9"] + ""));
                DriverContainer.Driver.Disconnect();
                return;
            }

            // Fire the event
            OnDataRetrievalCompletedEventHandler?.Invoke(this, null);
        }

        private bool ExecuteQuery(DateTime long_ago, DateTime right_now, string tableName)
        {
            List<MeasurePoint> _allData = new List<MeasurePoint>();
            // Read
            if (IsConnected)
            {
#if !DEMO
                try
                { 
                    _isRetrieving.WaitOne(); // Pause if there is someone already retrieving data
                    EventAggregator.SignalIsRetrievingData(CUSTOM, true);

                    string selectDataQuery = BuildQueryString(long_ago, right_now, tableName, N3PR_Data.REG_NAMES);
                    using (MySqlCommand msqlcmd = new MySqlCommand(selectDataQuery, _connection))
                    {
                        msqlcmd.CommandTimeout = 1000;
                        using (MySqlDataReader msqldatareader = msqlcmd.ExecuteReader())
                        {
                            // Parse data reader                            
                            _allData = ParseDataReader(msqldatareader);
                            lock (MbData)
                                MbData = MbData.Concat(SortRetrievedData(_allData)).ToList(); // Sort query (data)                                
                        }
                    }
                    
                    string selectAlarmQuery = BuildQueryString(long_ago, right_now, tableName, N3PR_Data.ALARM_WARNING_NAMES);
                    using (MySqlCommand msqlcmd = new MySqlCommand(selectAlarmQuery, _connection))
                    {
                        msqlcmd.CommandTimeout = 1000;
                        using (MySqlDataReader msqldatareader = msqlcmd.ExecuteReader())
                        {
                            // Parse data reader
                            _allData = ParseAlarmReader(msqldatareader);
                            lock (MbAlarm)
                                MbAlarm = MbAlarm.Concat(SortRetrievedAlarms(_allData)).ToList(); // Sort query (alarms)    
                        }
                    }
                    
                    if (!_isRetrieving.SafeWaitHandle.IsClosed)
                        _isRetrieving.Release(1);

                    EventAggregator.SignalIsRetrievingData(CUSTOM, false);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
#else
                lock (MbData)
                    MbData = MbData.Concat(SortRetrievedData(null)).ToList();

                lock (MbAlarm)
                    MbAlarm = MbAlarm.Concat(SortRetrievedAlarms(null)).ToList();

                return true;
#endif
            }
            else
                return false;                                            
        }

        private List<MeasurePoint> ParseDataReader(IDataReader _dataReader)
        {
            int digits = 1;
            List<MeasurePoint> _allData = new List<MeasurePoint>();                    
            while (_dataReader.Read())
            {
                int idxD = N3PR_Data.REG_NAMES.IndexOf(_dataReader[N3PR_DB.REG_NAME] + "");

                if (_dataReader[N3PR_DB.REG_NAME] + "" == "STE_IPS" || _dataReader[N3PR_DB.REG_NAME] + "" == "STE_OPS")
                    digits = 3;
                else
                    digits = 1;
                
                double value;
                if (idxD != -1)
                {
                    value = Math.Round(Convert.ToDouble(_dataReader[N3PR_DB.IVAL] + "") / 
                        Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture), digits);
                    
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
            return _allData;
        }

        private List<MeasurePoint> ParseAlarmReader(IDataReader _dataReader)
        {
            List<MeasurePoint> _allData = new List<MeasurePoint>();
            while (_dataReader.Read())
            {
                int idxA = N3PR_Data.ALARM_WARNING_NAMES.IndexOf(_dataReader[N3PR_DB.REG_NAME] + "");                

                double value;
                if (idxA != -1)
                {
                    value = Convert.ToDouble(_dataReader[N3PR_DB.IVAL] + "");
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

        private List<MeasurePoint> SortRetrievedData(List<MeasurePoint> aData)
        {
            List<MeasurePoint> _data = new List<MeasurePoint>();
#if !DEMO
            // Sort the retrieved entries (alarms or data?)
            if (aData.Count() > 0)
            {
                // data
                foreach (MeasurePoint _mbp in aData)
                    if (N3PR_Data.REG_NAMES.Contains(_mbp.Reg_Name))
                        _data.Add(_mbp);
            }
#else
            // Add 1000 datas
            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < N3PR_Data.REG_NAMES.Count(); j++)
                {
                    _data.Add(new MeasurePoint
                    {
                        Date = DateTime.Now,
                        Reg_Name = N3PR_Data.REG_NAMES[j],
                        data_type = N3PR_Data.REG_TYPES[j],
                        unit = N3PR_Data.REG_MEASUNIT[j],
                        val = (N3PR_Data.REG_TYPES[j] == N3PR_Data.BOOL) ? rnd.Next(1) : rnd.Next(5000)
                    });
                }
            }            
            Thread.Sleep(2000);           
#endif
            return _data;
        }

        private List<MeasurePoint> SortRetrievedAlarms(List<MeasurePoint> aData)
        {
            List<MeasurePoint> _alarms = new List<MeasurePoint>();
#if !DEMO
            // Sort the retrieved alarms
            if (aData.Count() > 0)
            {
                // alarms
                foreach (MeasurePoint _mbp in aData)
                    if (N3PR_Data.ALARM_WARNING_NAMES.Contains(_mbp.Reg_Name))
                        _alarms.Add(_mbp);
            }
#else
            // Add 20 datas
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    _alarms.Add(new MeasurePoint
                    {
                        Date = DateTime.Now,
                        Reg_Name = N3PR_Data.ALARM_NAMES[j],
                        data_type = N3PR_Data.ALARM_TYPES[j],
                        val = rnd.Next(2)
                    });
                }
            }
            Thread.Sleep(500);           
#endif
            return _alarms;
        }

        private bool SplitQuery(DateTime origin, DateTime end, string tableName, int split_days)
        {
            DateTime moving_origin = origin;
            DateTime moving_end = origin;

            while ((end.Subtract(moving_origin)).Days >= split_days)
            {
                moving_origin = moving_end;
                moving_end = moving_origin.AddDays(split_days);
                // Execute query
                if (!ExecuteQuery(moving_origin, moving_end, tableName))                                  
                    return false;                                
                OnDataRetrievalEventHandler?.Invoke(new EventWithMessage("", 100 * moving_end.Subtract(origin).Days / end.Subtract(origin).Days));
            }

            // Last query
            if ((end.Subtract(moving_origin)).Days < split_days)
            {
                // Execute query                
                if (!ExecuteQuery(moving_origin, end, tableName))
                    return false;
                OnDataRetrievalEventHandler?.Invoke(new EventWithMessage("", 100));
            }

            return true;
        }

        private string BuildQueryString(DateTime from, DateTime to, string tableName)
        {
            return "SELECT " + N3PR_DB.DATE + "," + N3PR_DB.REG_NAME + "," + N3PR_DB.IVAL + " FROM " +
                tableName + " WHERE " + N3PR_DB.DATE + " BETWEEN '" + from.ToString(N3PR_Data.DATA_FORMAT) +
                "' AND '" + to.ToString(N3PR_Data.DATA_FORMAT) + "'";
        }

        private string BuildQueryString(DateTime from, DateTime to, string tableName, List<string> regNames)
        {
            string regNamesJoinedOr = "";
            for (int i = 0; i < regNames.Count() - 1; i++)
                regNamesJoinedOr += N3PR_DB.REG_NAME + " = '" + regNames[i] + "' OR ";
            regNamesJoinedOr += N3PR_DB.REG_NAME + " = '" + regNames.Last() + "'";

            return "SELECT " + N3PR_DB.DATE + "," + N3PR_DB.REG_NAME + "," + N3PR_DB.IVAL + " FROM " +
                tableName + " WHERE " + N3PR_DB.DATE + " BETWEEN '" + from.ToString(N3PR_Data.DATA_FORMAT) +
                "' AND '" + to.ToString(N3PR_Data.DATA_FORMAT) + "' AND (" + regNamesJoinedOr + ")";
        }

        private string BuildNegativeQueryString(DateTime from, DateTime to, string tableName, List<string> regNames)
        {
            string regNamesJoinedAnd = "";
            for (int i = 0; i < regNames.Count() - 1; i++)
                regNamesJoinedAnd += N3PR_DB.REG_NAME + " != '" + regNames[i] + "' AND ";
            regNamesJoinedAnd += N3PR_DB.REG_NAME + " != '" + regNames.Last() + "'";

            return "SELECT " + N3PR_DB.DATE + "," + N3PR_DB.REG_NAME + "," + N3PR_DB.IVAL + " FROM " +
                tableName + " WHERE " + N3PR_DB.DATE + " BETWEEN '" + from.ToString(N3PR_Data.DATA_FORMAT) +
                "' AND '" + to.ToString(N3PR_Data.DATA_FORMAT) + "' AND (" + regNamesJoinedAnd + ")";
        }
    }    
}
