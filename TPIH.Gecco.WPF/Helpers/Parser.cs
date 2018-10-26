using System.Collections.Generic;
using System.Xml.Linq;

namespace TPIH.Gecco.WPF.Helpers
{
    static class Parser
    {
        public static List<string> ParseXmlElement(IEnumerable<XNode> nodes)
        {
            List<string> myS = new List<string>();
            foreach (XNode xn in nodes)
                myS.Add(xn.ToString());

            if (myS.Count > 0)
                return myS;
            else
                return null;
        }
    }
}
