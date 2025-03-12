using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAppTrip.Db;
using WpfAppTrip.Models;
using WpfAppTrip.Services;

namespace WpfAppTrip.ViewModels
{
    public class SurveyViewModel : INotifyPropertyChanged
    {
        private readonly Dbhelper _dbHelper;
        private readonly NavigationService _navigationService;
        private ObservableCollection<Question> _questions;
        private int _currentQuestionIndex;
        private Question _currentQuestion;
        private double _progress;
        private Thickness _progressIconMargin;

        public SurveyViewModel(NavigationService navigationService)
        {
            _dbHelper = new Dbhelper();
            _navigationService = navigationService;
            LoadQuestionsAsync();
            NextQuestionCommand = new RelayCommand(NextQuestion, CanGoToNextQuestion);
        }

        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                if (_currentQuestion != value)
                {
                    _currentQuestion = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CurrentQuestionNumber => _currentQuestionIndex + 1;

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                    UpdateProgressIconMargin();
                }
            }
        }

        public Thickness ProgressIconMargin
        {
            get => _progressIconMargin;
            set
            {
                if (_progressIconMargin != value)
                {
                    _progressIconMargin = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NextQuestionCommand { get; }

        private async void LoadQuestionsAsync()
        {
            try
            {
                // Загрузка вопросов из базы данных
                var questions = await _dbHelper.GetDataListAsync(
                    "SELECT * FROM Questions ORDER BY OrderNumber",
                    reader => new Question
                    {
                        QuestionID = reader.GetInt32(reader.GetOrdinal("QuestionID")),
                        QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                        OrderNumber = reader.GetInt32(reader.GetOrdinal("OrderNumber")),
                        HasImageOptions = reader.GetBoolean(reader.GetOrdinal("HasImageOptions"))
                    });

                _questions = new ObservableCollection<Question>(questions);

                // Загрузка вариантов ответов для каждого вопроса
                foreach (var question in _questions)
                {
                    var options = await _dbHelper.GetDataListAsync(
                        "SELECT * FROM AnswerOptions WHERE QuestionID = @QuestionID",
                        reader => new AnswerOption
                        {
                            OptionID = reader.GetInt32(reader.GetOrdinal("OptionID")),
                            QuestionID = reader.GetInt32(reader.GetOrdinal("QuestionID")),
                            OptionText = reader.GetString(reader.GetOrdinal("OptionText")),
                            ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                            Weight = reader.GetInt32(reader.GetOrdinal("Weight"))
                        },
                        new System.Collections.Generic.Dictionary<string, object> { { "@QuestionID", question.QuestionID } });

                    foreach (var option in options)
                    {
                        question.Options.Add(option);
                    }
                }

                // Установка первого вопроса
                if (_questions.Count > 0)
                {
                    _currentQuestionIndex = 0;
                    CurrentQuestion = _questions[0];
                    UpdateProgress();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вопросов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextQuestion(object parameter)
        {
            // Сохранение ответа пользователя
            SaveUserAnswer();

            // Переход к следующему вопросу
            if (_currentQuestionIndex < _questions.Count - 1)
            {
                _currentQuestionIndex++;
                CurrentQuestion = _questions[_currentQuestionIndex];
                UpdateProgress();
            }
            else
            {
                // Завершение опроса
                FinishSurvey();
            }
        }

        private bool CanGoToNextQuestion(object parameter)
        {
            return CurrentQuestion?.Options.Any(o => o.IsSelected) == true;
        }

        private void UpdateProgress()
        {
            Progress = ((_currentQuestionIndex + 1) * 100.0) / _questions.Count;
            OnPropertyChanged(nameof(CurrentQuestionNumber));
        }

        private void UpdateProgressIconMargin()
        {
            // Расчет позиции иконки прогресса
            double progressWidth = 500; // Примерная ширина прогресс-бара
            double iconPosition = (Progress * progressWidth) / 100;
            ProgressIconMargin = new Thickness(iconPosition - 12, -10, 0, 0); // -12 для центрирования иконки
        }

        private void SaveUserAnswer()
        {
            if (CurrentQuestion == null) return;

            var selectedOption = CurrentQuestion.Options.FirstOrDefault(o => o.IsSelected);
            if (selectedOption == null) return;

            // Здесь можно добавить код для сохранения ответа в базу данных
            // Например:
            /*
            await _dbHelper.ExecuteNonQueryAsync(
                "INSERT INTO UserAnswers (UserID, QuestionID, SelectedOptionID) VALUES (@UserID, @QuestionID, @SelectedOptionID)",
                new Dictionary<string, object>
                {
                    { "@UserID", CurrentUserID },
                    { "@QuestionID", CurrentQuestion.QuestionID },
                    { "@SelectedOptionID", selectedOption.OptionID }
                });
            */
        }

        private void FinishSurvey()
        {
            // Здесь можно добавить код для завершения опроса и перехода к результатам
            MessageBox.Show("Опрос завершен! Спасибо за участие.", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Переход на страницу результатов
            // _navigationService.Navigate(new ResultsPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 