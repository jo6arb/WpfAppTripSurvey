using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WpfAppTrip.Converters
{
    /// <summary>
    /// Конвертер, который обрабатывает null значения для изображений
    /// </summary>
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DependencyProperty.UnsetValue;

            try
            {
                string imagePath = value.ToString();
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
