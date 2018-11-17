using System;
using System.Windows;

namespace TPIH.Gecco.WPF.Helpers
{
    public static class SharedResourceDictionary
    {
        private static string language = "EN";

        public static string dictionary_IT = "..\\Resources\\StringResources-IT.xaml";
        public static string dictionary_EN = "..\\Resources\\StringResources-EN.xaml";

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
        private static ResourceDictionary _sharedDictionary;
    }
}
