using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LaserPewer
{
    public class SizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Size? size = value as Size?;
            if (size == null) return string.Empty;
            return "(" + size.Value.Width.ToString("F3") + " ," + size.Value.Height.ToString("F3") + ")";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
