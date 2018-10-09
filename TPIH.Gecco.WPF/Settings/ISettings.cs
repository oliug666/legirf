namespace TPIH.Gecco.WPF.Settings
{
    public interface ISettings
    {
        object this[string propertyName] { get; set; }

        void Save();
    }
}
