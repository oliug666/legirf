using System;
using System.Windows;
using System.Windows.Data;

namespace TPIH.Gecco.WPF.Helpers
{
    class TimeIntervalsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string s = value.ToString();
                if (s == "-1")
                    return SharedResourceDictionary.SharedDictionary["L_ComboBox_Custom"];
                else
                    return SharedResourceDictionary.SharedDictionary["L_ComboBox_Last"] + " " + s + " " + SharedResourceDictionary.SharedDictionary["L_ComboBox_Days"]; 
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
