namespace TPIH.Gecco.WPF.Helpers
{
    public class EventWithMessage
    {
        public string name { get; set; }
        public double value { get; set; }
        public EventWithMessage(string n, double v)
        {
            name = n;
            value = v;
        }
    }
}
