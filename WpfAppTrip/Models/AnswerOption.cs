namespace WpfAppTrip.Models
{
    public class AnswerOption
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string OptionText { get; set; }
        public string ImagePath { get; set; }
        public int Weight { get; set; }
        public bool HasImage { get; set; }
    }
} 