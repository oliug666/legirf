namespace TPIH.Gecco.WPF.Helpers
{
    public class EventWithMessage
    {
        public string name { get; set; }
        public bool value { get; set; }
        public EventWithMessage(string n, bool v)
        {
            name = n;
            value = v;
        }
    }
}
