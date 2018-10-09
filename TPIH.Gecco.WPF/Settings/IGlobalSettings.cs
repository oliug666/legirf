namespace TPIH.Gecco.WPF.Settings
{
    public interface IGlobalSettings
    {
        void Update(string settingName, object value);
        object Get(string settingName);
    }
}
