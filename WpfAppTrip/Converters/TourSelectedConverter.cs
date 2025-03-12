using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WpfAppTrip.Models;
using WpfAppTrip.ViewModels;

namespace WpfAppTrip.Converters
{
    public class TourSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TourViewModel viewModel && parameter is Tour tour)
            {
                return viewModel.IsSelected ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 