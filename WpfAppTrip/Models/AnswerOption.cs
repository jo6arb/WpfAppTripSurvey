using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppTrip.Models
{
    public class AnswerOption : INotifyPropertyChanged
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string OptionText { get; set; }
        public string ImagePath { get; set; }
        public int Weight { get; set; }
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 