using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TPIH.Gecco.WPF.Core
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (Equals(value, false))
                return true;
            if (Equals(value, true))
                return false;
            return null;
        }
    }
}
