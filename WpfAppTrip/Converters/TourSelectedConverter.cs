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
            if (value is ToursViewModel viewModel && parameter is Tour tour)
            {
                return viewModel.IsTourSelected(tour) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 