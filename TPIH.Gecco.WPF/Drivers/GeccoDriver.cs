using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using TPIH.Gecco.WPF.Interfaces;
using TPIH.Gecco.WPF.Models;
using System.Net;
using System.Data;
using TPIH.Gecco.WPF.Settings;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Helpers;

namespace TPIH.Gecco.WPF.Drivers
{
    public class GeccoDriver : IGeccoDriver
    {
        private readonly GlobalSettings _settings = new GlobalSettings(new AppSettings());
        private MySqlConnection _connection;
        private MySqlCommand _cmd;
        private MySqlDataReader _dataReader;

        private static IList<MeasurePoint> _mbData;
        private static IList<MeasurePoint> _mbAlarm;
        private static IList<MeasurePoint> _latestData;
        private Semaphore _isRetrieving = new Semaphore(1,1);

        public bool IsConnected { get { return (_connection != null && (_connection.State == ConnectionState.Open)); } }
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
                    // GlobalCommands.ShowError.Execute(new Exception("Failed to retrieve data from server. Check network connection and try to re-connect."));
                    return null;
                }
            }
            private set { }
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
                    // GlobalCommands.ShowError.Execute(new Exception("Failed to retrieve data from server. Check network connection and try to re-connect."));
                    return null;
                }
            }
            private set { }
        }

        public IList<MeasurePoint> LatestData
        {
            get
            {
                if (IsConnected && _latestData.Count > 0)
                {
                    return _latestData;
                }
                else
                {
                    // GlobalCommands.ShowError.Execute(new Exception("Failed to retrieve data from server. Check network connection and try to re-connect."));
                    return null;
                }
            }
            private set { }
        }

        public GeccoDriver()
        {
        }

        private void Initialize(string ipAddress, int port, string dbname, string username, string password)
        {
            string connectionString = "SERVER=" + ipAddress + ";" + "PORT=" + port.ToString() + ";" + "DATABASE=" +
                dbname + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";";
            _connection = new MySqlConnection(connectionString);

            _mbData = new List<MeasurePoint>();
            _mbAlarm = new List<MeasurePoint>();
            _latestData = new List<MeasurePoint>();
        }

        public void Connect(string ipAddress, int port, string dbname, string username, string password)
        {
            Initialize(ipAddress, port, dbname, username, password);

            if (!IsConnected)
            {
                try
                {
                    _connection.Open();
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
                            Status = "Cannot connect to server.  Contact administrator";
                            break;

                        case 1045:
                            Status = "Invalid username/password, please try again";
                            break;
                    }
                }
            }

            OnConnectionStatusChanged?.Invoke(this, null);
        }

        //Close connection
        public void Disconnect()
        {           
            if (IsConnected)
            {
                _isRetrieving.WaitOne();
                try
                {
                    _connection.Close();                    
                }
                catch (MySqlException ex)
                {
                    Status = ex.Message;
                }
                _isRetrieving.Release(1);
            }
          
            OnConnectionStatusChanged?.Invoke(this, null);
        }

        public void Dispose()
        {
            try
            {
                if (_connection != null)
                    _connection.Dispose();
            }
            catch (MySqlException ex)
            {
                Status = ex.Message;
            }
        }

        public void GetLatestData(string tableName)
        {
            DateTime LatestDate = new DateTime();
            Thread.Sleep(1000);

            _isRetrieving.WaitOne(); // Pause if there is someone already retrieving data

            _dataReader = null;

            if (IsConnected & _latestData != null)
            {
                // First find the latest date            
                string dateQuery = "SELECT MAX(" + N3PR_DB.DATE + ") FROM " + tableName;
                try
                {
                    _cmd = new MySqlCommand(dateQuery, _connection);
                    var date = _cmd.ExecuteScalar();
                    LatestDate = ParseDate(date + "");                    
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(new Exception(e.Message + " - Error when trying to find newest date."));
                    DriverContainer.Driver.Disconnect();
                    _isRetrieving.Release(1);
                    return;
                }
                _cmd.Dispose();

                string selectQuery = "SELECT * FROM " + tableName + " WHERE " + tableName + "." + N3PR_DB.DATE
                    + " >= '" + LatestDate.AddSeconds(-10).ToString(N3PR_Data.DATA_FORMAT) + "'";

                // Pause if there is someone already retrieving data
                try
                {
                    lock (_latestData)
                    {
                        _cmd = new MySqlCommand(selectQuery, _connection);
                        _dataReader = _cmd.ExecuteReader();
                        _latestData.Clear();
                        _latestData = ParseDataReader(_dataReader);
                    }
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(new Exception(e.Message + " - Error when trying to retrive latest data."));
                }
            }

            _cmd.Dispose();
            if (_dataReader != null)
            {
                _dataReader.Close();
                _dataReader.Dispose();
            }
            _dataReader = null;

            // Fire the event
            _isRetrieving.Release(1);
            OnLatestDataRetrievalCompleted?.Invoke(this, null);           
        }

        public void GetDataFromLastXDays(string tableName, int lastDays)
        {
            _cmd = new MySqlCommand();
            _dataReader = null;

            _isRetrieving.WaitOne(); // Pause if there is someone already retrieving data

            // Get day today and calculate time interval
            DateTime right_now = DateTime.Now;
            string right_now_s = right_now.ToString(N3PR_Data.DATA_FORMAT);
            DateTime long_ago = DateTime.Now.AddDays(-lastDays);
            string long_ago_s = long_ago.ToString(N3PR_Data.DATA_FORMAT);

            // Create query
            string selectQuery = "SELECT * FROM " + tableName + " WHERE "+ N3PR_DB.DATE +
                " BETWEEN '" + long_ago_s + "' AND '" + right_now_s + "'";
            
            // Read
            ExecuteQuery(selectQuery);

            _isRetrieving.Release(1);
            // Fire the event
            OnDataRetrievalCompleted?.Invoke(this, null);
        }

        public void GetDataFromCalendarDays(string tableName, DateTime From, DateTime To)
        {
            _cmd = new MySqlCommand();
            _dataReader = null;

            _isRetrieving.WaitOne(); // Pause if there is someone already retrieving data

            // Create query     
            string selectQuery = "SELECT * FROM " + tableName + " WHERE " + N3PR_DB.DATE +
                " BETWEEN '" + From.ToString(N3PR_Data.DATA_FORMAT) + "' AND '" + 
                To.AddHours(23).AddMinutes(59).AddSeconds(59).ToString(N3PR_Data.DATA_FORMAT) + "'";

            // Read
            ExecuteQuery(selectQuery);

            _isRetrieving.Release(1);
            // Fire the event
            OnDataRetrievalCompleted?.Invoke(this, null);
        }

        private void ExecuteQuery(string selectQuery)
        {            
            // Read
            if (IsConnected)
            {
                try
                {                    
                    // Clear the previous data
                    _mbData.Clear();
                    _mbAlarm.Clear();

                    _cmd = new MySqlCommand(selectQuery, _connection);
                    _dataReader = _cmd.ExecuteReader();

                    // Parse data reader
                    List<MeasurePoint> _allData = ParseDataReader(_dataReader);

                    // Sort the retrieved entries (alarms or data?)
                    if (_allData.Count() > 0 & _allData != null)
                    {
                        // data
                        foreach (MeasurePoint _mbp in _allData)
                        {
                            if (N3PR_Data.REG_NAMES.Contains(_mbp.Reg_Name))
                                _mbData.Add(_mbp);
                        }
                        // alarms
                        foreach (MeasurePoint _mbp in _allData)
                        {
                            if (N3PR_Data.ALARM_NAMES.Contains(_mbp.Reg_Name))
                                _mbAlarm.Add(_mbp);
                        }
                    }
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(new Exception(e.Message + " - Error when trying to execute SQL query."));
                }
            }

            // Dispose connection objects
            _cmd.Dispose();
            if (_dataReader != null)
            {
                _dataReader.Close();
                _dataReader.Dispose();
            }
            _dataReader = null;
        }

        private List<MeasurePoint> ParseDataReader(IDataReader _dataReader)
        {
            List<MeasurePoint> _allData = new List<MeasurePoint>();
            while (_dataReader.Read())
            {
                int idxD = N3PR_Data.REG_NAMES.IndexOf(_dataReader[N3PR_DB.REG_NAME] + "");
                int idxA = N3PR_Data.ALARM_NAMES.IndexOf(_dataReader[N3PR_DB.REG_NAME] + "");

                double value;
                if (idxD != -1)
                {
                    switch (N3PR_Data.REG_TYPES[idxD])
                    {
                        case N3PR_Data.INT:
                            value = Convert.ToInt32(_dataReader[N3PR_DB.IVAL] + "") / Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture);
                            break;
                        case N3PR_Data.UINT:
                            value = Convert.ToUInt32(_dataReader[N3PR_DB.UIVAL] + "") / Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture);
                            break;
                        case N3PR_Data.BOOL:
                            value = Convert.ToDouble(_dataReader[N3PR_DB.BVAL] + "");
                            break;
                        default:
                            value = Convert.ToInt32(_dataReader[N3PR_DB.IVAL] + "") / Convert.ToDouble(N3PR_Data.REG_DIVFACTORS[idxD], CultureInfo.InvariantCulture);
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
                    if (N3PR_Data.ALARM_NAMES.Contains(_dataReader[N3PR_DB.REG_NAME] + ""))
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
    }    
}
