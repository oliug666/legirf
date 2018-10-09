using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class DisplayFirmwareViewModel : ViewModelBase
    {
        private const byte NumberOfBytesPerPackage = 100;

        private readonly AutoResetEvent _ackTrigger = new AutoResetEvent(false);
        private bool _commandSuccess = false;
        private byte _commandLength = 0;

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged(() => FilePath);
            }
        }

        private bool _isUploadEnabled;
        public bool IsUploadEnabled
        {
            get { return _isUploadEnabled; }
            set
            {
                _isUploadEnabled = value;
                OnPropertyChanged(() => IsUploadEnabled);
            }
        }

        #region Commands

        public ICommand BrowseCommand { get; private set; }
        public ICommand ServiceCommand { get; private set; }
        public ICommand UploadCommand { get; private set; }

        #endregion

        public DisplayFirmwareViewModel()
        {
            DriverContainer.Driver.OnCommandReceived += Driver_OnCommandReceived;

            BrowseCommand = new DelegateCommand(Browse);
            ServiceCommand = new DelegateCommand(Service, obj => DriverContainer.Driver.IsConnected);
            UploadCommand = new DelegateCommand(Upload, obj => DriverContainer.Driver.IsConnected);
        }



        void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            if (e.Info == InfoEnum.FirmwareUpdate)
            {
                var arguments = e.Message.Split(';');
                _commandLength = byte.Parse(arguments[0]);
                byte commandResponse = byte.Parse(arguments[1]);
                _commandSuccess = commandResponse == 6;
                _ackTrigger.Set();
            }
        }
        private void Upload(object obj)
        {
            Task.Factory.StartNew(UploadInBackground);
        }

        private void UploadInBackground()
        {
            try
            {
                if (DriverContainer.Driver.IsConnected)
                {
                    byte[] bytes = File.ReadAllBytes(FilePath);
                    var cmd = new List<byte>();

                    foreach (var c in bytes)
                    {
                        cmd.Add(c);
                        if (cmd.Count == NumberOfBytesPerPackage)
                        {
                            WriteDataAndVerify(cmd.ToArray(), cmd.Count);
                            cmd.Clear();
                        }
                    }
                    WriteDataAndVerify(cmd.ToArray(), cmd.Count);
                    cmd.Clear();
                }
                else
                {
                    MessageBox.Show("Not connected to device");
                }

            }
            catch (Exception exp)
            {
                GlobalCommands.ShowError.Execute(exp);
            }
        }
        private bool WriteDataAndVerify(byte[] data, int nbrOfBytes)
        {
            DriverContainer.Driver.Write(data, 0, nbrOfBytes);
            while (DriverContainer.Driver.BytesToWrite > 0) { }
            var signaled = _ackTrigger.WaitOne(TimeSpan.FromSeconds(10));
            if (!signaled)
            {
                throw new ArgumentException("Command Error: Timeout");
            }
            if (nbrOfBytes != _commandLength)
            {
                throw new ArgumentException("Command Error: Number of bytes did not match");
            }
            if (!_commandSuccess)
            {
                throw new ArgumentException("Command Error: SPI Communication Failed");
            }

            return true;
        }

        public bool ReadData(byte[] responseBytes, int bytesExpected)
        {
            int offset = 0, bytesRead;
            while (bytesExpected > 0 &&
              (bytesRead = DriverContainer.Driver.Read(responseBytes, offset, bytesExpected)) > 0)
            {
                offset += bytesRead;
                bytesExpected -= bytesRead;
            }
            return bytesExpected == 0;
        }
        private void Service(object obj)
        {
            DriverContainer.Driver.SetServiceState(true);
        }

        private void Browse(object obj)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Display Firmware File (.EEP)|*.EEP"
            };

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FilePath = dialog.FileName;
            }
        }
    }
}
