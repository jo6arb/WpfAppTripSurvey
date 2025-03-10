using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAppTrip.Converters
{
    /// <summary>
    /// Конвертер для преобразования логического значения в отступы
    /// </summary>
    public class BooleanToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // По умолчанию отступы для true и false
                var trueMargin = new Thickness(0);
                var falseMargin = new Thickness(0);

                // Если передан параметр в виде строки с форматом "left,top,right,bottom:left,top,right,bottom"
                if (parameter is string paramString)
                {
                    var parts = paramString.Split(':');
                    if (parts.Length == 2)
                    {
                        var trueParts = parts[0].Split(',');
                        var falseParts = parts[1].Split(',');

                        if (trueParts.Length == 4 && falseParts.Length == 4)
                        {
                            trueMargin = new Thickness(
                                double.Parse(trueParts[0], CultureInfo.InvariantCulture),
                                double.Parse(trueParts[1], CultureInfo.InvariantCulture),
                                double.Parse(trueParts[2], CultureInfo.InvariantCulture),
                                double.Parse(trueParts[3], CultureInfo.InvariantCulture));

                            falseMargin = new Thickness(
                                double.Parse(falseParts[0], CultureInfo.InvariantCulture),
                                double.Parse(falseParts[1], CultureInfo.InvariantCulture),
                                double.Parse(falseParts[2], CultureInfo.InvariantCulture),
                                double.Parse(falseParts[3], CultureInfo.InvariantCulture));
                        }
                    }
                }

                return boolValue ? trueMargin : falseMargin;
            }

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 