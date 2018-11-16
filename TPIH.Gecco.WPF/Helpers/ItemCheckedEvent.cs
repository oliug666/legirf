namespace TPIH.Gecco.WPF.Helpers
{
    public class ItemCheckedEvent
    {
        public string name { get; set; }
        public bool value { get; set; }
        public ItemCheckedEvent(string n, bool v)
        {
            name = n;
            value = v;
        }
    }
}
