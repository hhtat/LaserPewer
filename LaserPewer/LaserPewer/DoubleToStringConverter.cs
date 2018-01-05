using System;
using System.Globalization;
using System.Windows.Data;

namespace LaserPewer
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString("F");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double _value;
            double.TryParse((string)value, out _value);
            return _value;
        }
    }
}
