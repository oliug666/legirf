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
        private static IList<MeasurePoint> _mbData;
        private static IList<MeasurePoint> _mbAlarm;
        private static IList<MeasurePoint> _latestData;
        private bool _isRetrieving = false;

        public bool IsConnected { get { return (_connection != null && (_connection.State == ConnectionState.Open)); } }        
        public string Status;
        public event EventHandler OnDataRetrievalCompleted;
        public event EventHandler OnLatestDataRetrievalCompleted;

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

            //if (_connection.State == ConnectionState.Open)
            //{
            //    _mbData = new List<MeasurePoint>();
            //    _latestData = new List<MeasurePoint>();
            //}
        }

        //Close connection
        public void Disconnect()
        {
            if (IsConnected)
            {
                try
                {
                    _connection.Close();                    
                }
                catch (MySqlException ex)
                {
                    Status = ex.Message;
                }
            }
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
            while (_isRetrieving) ;
            
            if (IsConnected & _latestData != null)
            {
                // First find the latest date            
                string dateQuery = "SELECT MAX(DATE) FROM " + tableName;
                try
                {
                    _isRetrieving = true;                    
                    _cmd = new MySqlCommand(dateQuery, _connection);
                    var date = _cmd.ExecuteScalar();                      
                    try
                    {
                        LatestDate = DateTime.ParseExact(date + "", N3PR_Data.DATA_FORMAT,
                            System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        LatestDate = DateTime.ParseExact(date + "", N3PR_Data.DATA_FORMAT,
                            System.Globalization.CultureInfo.CurrentCulture);
                    }
                    
                    _cmd.Dispose();
                }
                catch (Exception e)
                {                    
                    GlobalCommands.ShowError.Execute(e);                    
                    return;
                }

                string selectQuery = "SELECT * FROM " + tableName + " WHERE " + tableName + ".DATE >= '" + LatestDate.AddSeconds(-10).ToString(N3PR_Data.DATA_FORMAT) + "'";

                try
                {
                    _isRetrieving = true;
                    MySqlDataReader _dataReader;
                    lock (_latestData)
                    {
                        _cmd = new MySqlCommand(selectQuery, _connection);
                        _dataReader = _cmd.ExecuteReader();
                        _latestData.Clear();
                        while (_dataReader.Read())
                        {
                            DateTime dd;
                            try
                            {
                                dd = DateTime.ParseExact(_dataReader["DATE"] + "", N3PR_Data.DATA_FORMAT,
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                dd = DateTime.ParseExact(_dataReader["DATE"] + "", N3PR_Data.DATA_FORMAT,
                                    System.Globalization.CultureInfo.CurrentCulture);
                            }
                            if (N3PR_Data.REG_NAMES.Contains(_dataReader["REG_NAME"]))
                            {
                                _latestData.Add(new MeasurePoint
                                {
                                    Date = dd,
                                    Reg_Name = _dataReader["REG_NAME"] + "",
                                    i_val = Convert.ToInt32(_dataReader["I_VAL"] + ""),
                                    ui_val = Convert.ToInt32(_dataReader["UI_VAL"] + ""),
                                    b_val = (_dataReader["B_VAL"] + "") == "1"
                                });
                            }
                        }
                        _cmd.Dispose();
                        _dataReader.Close();
                        _dataReader.Dispose();
                    }                 
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(e);
                }
            }

            _isRetrieving = false;
            // Fire the event
            OnLatestDataRetrievalCompleted?.Invoke(this, null);
        }

        public void GetDataFromLastXDays(string tableName, int lastDays)
        {
            while (_isRetrieving) ;

            // Get day today and calculate time interval
            DateTime right_now = DateTime.Now;
            string right_now_s = right_now.ToString(N3PR_Data.DATA_FORMAT);
            DateTime long_ago = DateTime.Now.AddDays(-lastDays);
            string long_ago_s = long_ago.ToString(N3PR_Data.DATA_FORMAT);

            string selectQuery = "SELECT * FROM " + tableName + " WHERE DATE BETWEEN '" + long_ago_s + "' AND '" + right_now_s + "'";
            // Read
            if (IsConnected)
            {
                try
                {
                    _isRetrieving = true;
                    // Clear the previous data
                    _mbData.Clear();
                    _mbAlarm.Clear();

                    MySqlDataReader _dataReader;                    
                    _cmd = new MySqlCommand(selectQuery, _connection);
                    _dataReader = _cmd.ExecuteReader();
                    List<MeasurePoint> _allData = new List<MeasurePoint>();

                    while (_dataReader.Read())
                    {
                        _allData.Add(new MeasurePoint
                        {
                            Date = DateTime.ParseExact(_dataReader["DATE"] + "", N3PR_Data.DATA_FORMAT,
                                System.Globalization.CultureInfo.InvariantCulture),
                            Reg_Name = _dataReader["REG_NAME"] + "",
                            i_val = Convert.ToInt32(_dataReader["I_VAL"] + ""),
                            ui_val = Convert.ToInt32(_dataReader["UI_VAL"] + ""),
                            b_val = (_dataReader["B_VAL"] + "") == "1"
                        });
                    }
                    _cmd.Dispose();
                    _dataReader.Close();
                    _dataReader.Dispose();
                    _dataReader = null;

                    if (_allData.Count() > 0 & _allData != null)
                    {
                        // Process the data
                        foreach (MeasurePoint _mbp in _allData)
                        {
                            if (N3PR_Data.REG_NAMES.Contains(_mbp.Reg_Name))
                                _mbData.Add(_mbp);
                        }
                        // Process the alarms
                        foreach (MeasurePoint _mbp in _allData)
                        {
                            if (N3PR_Data.ALARM_NAMES.Contains(_mbp.Reg_Name))
                                _mbAlarm.Add(_mbp);
                        }
                    }
                }
                catch (Exception e)
                {
                    GlobalCommands.ShowError.Execute(e);
                }                
            }

            _isRetrieving = false;
            // Fire the event
            OnDataRetrievalCompleted?.Invoke(this, null);
        }        
    }
}
