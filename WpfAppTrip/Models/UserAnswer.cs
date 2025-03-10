using System;

namespace WpfAppTrip.Models
{
    public class UserAnswer
    {
        public int AnswerID { get; set; }
        public int UserID { get; set; }
        public int QuestionID { get; set; }
        public int OptionID { get; set; }
        public DateTime AnswerDate { get; set; }
        
        // Навигационные свойства
        public User User { get; set; }
        public Question Question { get; set; }
        public AnswerOption Option { get; set; }
    }
} 