namespace WpfAppTrip.Models
{
    public class TourCharacteristic
    {
        public int ID { get; set; }
        public int TourID { get; set; }
        public int CategoryID { get; set; }
        public int Weight { get; set; }
        
        // Навигационные свойства
        public Tour Tour { get; set; }
        public TourCategory Category { get; set; }
    }
} 