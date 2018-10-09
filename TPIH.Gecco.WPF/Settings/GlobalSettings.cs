using System;

namespace TPIH.Gecco.WPF.Settings
{
    public class GlobalSettings : IGlobalSettings
    {
        public string IPAddress
        {
            get { return (string) Get("IPAddress"); }
            set { Update("IPAddress", value);}
        }
        public string Port
        {
            get { return (string)Get("Port"); }
            set { Update("Port", value); }
        }
        public string DBname
        {
            get { return (string)Get("DBname"); }
            set { Update("DBname", value); }
        }
        public string TableName
        {
            get { return (string)Get("TableName"); }
            set { Update("TableName", value); }
        }
        public string Username
        {
            get { return (string)Get("Username"); }
            set { Update("Username", value); }
        }
        public string Password
        {
            get { return (string)Get("Password"); }
            set { Update("Password", value); }
        }

        public string DefaultPath
        {
            get { return (string)Get("DefaultPath"); }
            set { Update("DefaultPath", value); }
        }

        public string DefaultFileName
        {
            get { return (string)Get("DefaultFileName"); }
            set { Update("DefaultFileName", value); }
        }

        protected ISettings Settings;
        public GlobalSettings(ISettings settings)
        {
            Settings = settings;
        }

        public void Update(string settingName, object value)
        {
            if (String.IsNullOrEmpty(settingName))
                throw new ArgumentNullException("Setting name must be provided");

            var Setting = Settings[settingName];

            if (Setting == null)
            {
                throw new ArgumentException("Setting " + settingName + " not found.");
            }
            if (Setting.GetType() != value.GetType())
            {
                throw new InvalidCastException("Unable to cast value to " + Setting.GetType());
            }
            Settings[settingName] = value;
            Settings.Save();
        }

        public object Get(string settingName)
        {
            if (String.IsNullOrEmpty(settingName))
                throw new ArgumentNullException("Setting name must be provided");

            return Settings[settingName];
        }
    }
}
