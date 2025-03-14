using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAppTrip.Converters
{
    /// <summary>
    /// Конвертер, который преобразует 0 в Visibility.Visible, а не-0 в Visibility.Collapsed
    /// </summary>
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 