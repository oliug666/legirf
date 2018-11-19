using System;
using System.Windows;

namespace TPIH.Gecco.WPF.Helpers
{
    public static class SharedResourceDictionary
    {
        public static string language = "EN";

        public static string dictionary_IT = "..\\Resources\\Resources.it-IT.xaml";
        public static string dictionary_EN = "..\\Resources\\Resources.en-US.xaml";

        public static string dictionary
        {
            get
            {
                switch (language)
                {
                    case "IT":
                        return dictionary_IT;
                    case "EN":
                        return dictionary_EN;
                    default:
                        return dictionary_EN;
                }
            }
        }

        public static ResourceDictionary SharedDictionary
        {
            get
            {
                if (_sharedDictionary == null)
                {
                    try
                    {
                        System.Uri resourceLocater1 = new System.Uri(dictionary, System.UriKind.Relative);
                        ResourceDictionary resourceDictionary = new ResourceDictionary
                        {
                            Source = resourceLocater1
                        };
                        _sharedDictionary = resourceDictionary;
                    }
                    catch (Exception e)
                    {

                    }
                }

                return _sharedDictionary;
            }
        }

        public static void ChangeLocalization(string loc)
        {
            language = loc;
        }

        private static ResourceDictionary _sharedDictionary;
    }
}
