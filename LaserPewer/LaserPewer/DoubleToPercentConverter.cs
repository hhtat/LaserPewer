using System;
using System.Globalization;
using System.Windows.Data;

namespace LaserPewer
{
    public class DoubleToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (100.0 * (double)value).ToString("F0") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
