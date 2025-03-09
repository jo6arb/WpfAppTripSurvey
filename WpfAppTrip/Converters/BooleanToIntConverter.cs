using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfAppTrip.Converters
{
    public class BooleanToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string paramString)
                {
                    var parts = paramString.Split(':');
                    if (parts.Length == 2)
                    {
                        return boolValue ? int.Parse(parts[0]) : int.Parse(parts[1]);
                    }
                }
                return boolValue ? 1 : 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 