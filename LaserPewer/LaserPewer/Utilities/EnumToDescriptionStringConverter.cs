using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace LaserPewer.Utilities
{
    public class EnumToDescriptionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value.GetType();
            string name = type.GetEnumName(value);
            MemberInfo[] members = type.GetMember(name);
            DescriptionAttribute description = members[0].GetCustomAttribute<DescriptionAttribute>();
            return description != null ? description.Description : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
