using System;

namespace WpfAppTrip.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int UserID { get; set; }
        public int TourID { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime TourDate { get; set; }
        public string Status { get; set; }
        
        // Навигационные свойства
        public User User { get; set; }
        public Tour Tour { get; set; }
    }
} 