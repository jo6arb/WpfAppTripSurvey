using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAppTrip.Converters
{
    public class BooleanToMarginConverter : IValueConverter
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
                        var trueMargin = parts[0].Split(',');
                        var falseMargin = parts[1].Split(',');
                        
                        if (boolValue && trueMargin.Length == 4)
                        {
                            return new Thickness(
                                double.Parse(trueMargin[0]),
                                double.Parse(trueMargin[1]),
                                double.Parse(trueMargin[2]),
                                double.Parse(trueMargin[3])
                            );
                        }
                        else if (!boolValue && falseMargin.Length == 4)
                        {
                            return new Thickness(
                                double.Parse(falseMargin[0]),
                                double.Parse(falseMargin[1]),
                                double.Parse(falseMargin[2]),
                                double.Parse(falseMargin[3])
                            );
                        }
                    }
                }
                return boolValue ? new Thickness(20, 0, 0, 0) : new Thickness(0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 