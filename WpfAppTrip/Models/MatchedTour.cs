using System;

namespace WpfAppTrip.Models
{
    public class MatchedTour
    {
        public int MatchID { get; set; }
        public int UserID { get; set; }
        public int TourID { get; set; }
        public double MatchScore { get; set; }
        public DateTime MatchDate { get; set; }
        
        // Навигационные свойства
        public User User { get; set; }
        public Tour Tour { get; set; }
    }
} 