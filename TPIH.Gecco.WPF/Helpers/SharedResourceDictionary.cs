using System;
using System.Collections.Generic;
using System.Windows;

namespace TPIH.Gecco.WPF.Helpers
{
    public static class SharedResourceDictionary
    {                        
        public const string dictionary_EN = "en-US";
        public const string dictionary_IT = "it-IT";

        private static string language = dictionary_EN;

        public static string dictionary
        {
            get
            {
                switch (language)
                {
                    case dictionary_IT:
                        return "..\\Resources\\Resources." + dictionary_IT + ".xaml";
                    case dictionary_EN:
                        return "..\\Resources\\Resources." + dictionary_EN + ".xaml";
                    default:
                        return "..\\Resources\\Resources." + dictionary_EN + ".xaml";
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
                        Uri resourceLocater1 = new Uri(dictionary, System.UriKind.Relative);
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

        public static List<string> AvailableLanguages = new List<string>()
        {
            (string)SharedDictionary["L_English"],
            (string)SharedDictionary["L_Italian"]
        };

        public static void ChangeLanguage(string val)
        {
            language = val;
            if (_sharedDictionary != null)
            {
                try
                {
                    Uri resourceLocater1 = new Uri(dictionary, System.UriKind.Relative);
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
        }

    }
}
