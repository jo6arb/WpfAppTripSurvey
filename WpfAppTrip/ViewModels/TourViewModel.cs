using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfAppTrip.Models;

namespace WpfAppTrip.ViewModels
{
    public class TourViewModel : BaseViewModel
    {
        private readonly Tour _tour;
        private bool _isSelected;

        public TourViewModel(Tour tour)
        {
            _tour = tour ?? throw new ArgumentNullException(nameof(tour));
        }

        public int TourID => _tour.TourID;
        public string Name => _tour.Name;
        public string Description => _tour.Description;
        public decimal Price => _tour.Price;
        public string ImagePath => _tour.ImagePath;
        public int Duration => _tour.Duration;
        public string Season => _tour.Season;
        public string Difficulty => _tour.Difficulty;
        public int MaxGroupSize => _tour.MaxGroupSize;
        public bool HasImage => _tour.HasImage;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public Tour Tour => _tour;
    }
} 