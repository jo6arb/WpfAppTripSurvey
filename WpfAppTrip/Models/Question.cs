using System.Collections.ObjectModel;

namespace WpfAppTrip.Models
{
    public class Question
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string ImagePath { get; set; }
        public int OrderNumber { get; set; }
        public bool HasImageOptions { get; set; }
        public ObservableCollection<AnswerOption> Options { get; set; } = new ObservableCollection<AnswerOption>();
    }
} 