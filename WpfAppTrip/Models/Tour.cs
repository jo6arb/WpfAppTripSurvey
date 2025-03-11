using System.Collections.Generic;

namespace WpfAppTrip.Models
{
    public class Tour
    {
        public int TourID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public int Duration { get; set; }
        public string Season { get; set; }
        public string Difficulty { get; set; }
        public int MaxGroupSize { get; set; }
        
        // Навигационные свойства (заполняются отдельно)
        public List<TourCategory> Categories { get; set; }
        public List<TourCharacteristic> Characteristics { get; set; }
        
        public Tour()
        {
            Categories = new List<TourCategory>();
            Characteristics = new List<TourCharacteristic>();
        }
        
        // Вычисляемое свойство для проверки наличия изображения
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);
    }
} 