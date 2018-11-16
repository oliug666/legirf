using System;
using System.Windows;
using System.Windows.Data;

namespace TPIH.Gecco.WPF.Helpers
{
    class TimeIntervalsConverter : IValueConverter
    {
        readonly ResourceDictionary resourceDictionary = (ResourceDictionary)SharedResourceDictionary.SharedDictionary;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string s = value.ToString();
                if (s == "-1")
                    return resourceDictionary["L_ComboBox_Custom"];
                else
                    return resourceDictionary["L_ComboBox_Last"] + " " + s + " " + resourceDictionary["L_ComboBox_Days"]; 
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
