using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using WpfAppTrip.Db;
using System.Threading.Tasks;
using WpfAppTrip.Services;
using WpfAppTrip.ViewModels;
using System.Windows.Input;
using WpfAppTrip.Models;

namespace WpfAppTrip.Views.Pages
{
    public partial class SurveyPage : Page
    {
        private readonly Dbhelper _db;
        private List<Question> _questions;
        private List<AnswerOption> _currentAnswerOptions;
        private int _currentQuestionIndex = 0;
        private Dictionary<int, int> _userAnswers = new Dictionary<int, int>();
        private bool _isAnswerSelected = false;

        public SurveyPage()
        {
            InitializeComponent();
            _db = new Dbhelper();
            var navigationService = new NavigationService(null); // Здесь нужно передать Frame из MainWindow
            DataContext = new SurveyViewModel(navigationService);
            LoadQuestionsAsync();
        }

        public string CurrentQuestionText => $"Вопрос {_currentQuestionIndex + 1} из {_questions?.Count ?? 0}";
        public Question CurrentQuestion => _questions?[_currentQuestionIndex];
        public IEnumerable<AnswerOption> CurrentAnswerOptions => _currentAnswerOptions;
        public bool IsAnswerSelected => _isAnswerSelected;
        public bool IsPreviousButtonVisible => _currentQuestionIndex > 0;
        public string NextButtonText => _currentQuestionIndex == (_questions?.Count - 1) ? "Завершить" : "Далее";

        private async void LoadQuestionsAsync()
        {
            try
            {
                // Загрузка вопросов
                _questions = await _db.GetDataListAsync<Question>(
                    "SELECT * FROM Questions ORDER BY OrderNumber",
                    reader => new Question
                    {
                        QuestionID = reader.GetInt32(0),
                        QuestionText = reader.GetString(1),
                        ImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                        OrderNumber = reader.GetInt32(3)
                    }
                );

                if (_questions.Any())
                {
                    await LoadAnswerOptionsForQuestionAsync(_questions[0].QuestionID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вопросов: {ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private async Task LoadAnswerOptionsForQuestionAsync(int questionId)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@QuestionID", questionId }
                };

                _currentAnswerOptions = await _db.GetDataListAsync<AnswerOption>(
                    "SELECT * FROM AnswerOptions WHERE QuestionID = @QuestionID",
                    reader => new AnswerOption
                    {
                        OptionID = reader.GetInt32(0),
                        QuestionID = reader.GetInt32(1),
                        OptionText = reader.GetString(2),
                        ImagePath = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Weight = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                    },
                    parameters
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вариантов ответа: {ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                _isAnswerSelected = true;
                int answerId = (int)radioButton.Tag;
                _userAnswers[CurrentQuestion.QuestionID] = answerId;
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex < _questions.Count - 1)
            {
                _currentQuestionIndex++;
                await LoadAnswerOptionsForQuestionAsync(_questions[_currentQuestionIndex].QuestionID);
                _isAnswerSelected = _userAnswers.ContainsKey(_questions[_currentQuestionIndex].QuestionID);
            }
            else
            {
                await SaveSurveyResultsAsync();
                NavigateToResults();
            }
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                await LoadAnswerOptionsForQuestionAsync(_questions[_currentQuestionIndex].QuestionID);
                _isAnswerSelected = _userAnswers.ContainsKey(_questions[_currentQuestionIndex].QuestionID);
            }
        }

        private async Task SaveSurveyResultsAsync()
        {
            try
            {
                foreach (var answer in _userAnswers)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@UserID", 1 }, // Здесь нужно передать реальный ID пользователя
                        { "@QuestionID", answer.Key },
                        { "@SelectedOptionID", answer.Value }
                    };

                    await _db.ExecuteNonQueryAsync(
                        "INSERT INTO UserAnswers (UserID, QuestionID, SelectedOptionID) VALUES (@UserID, @QuestionID, @SelectedOptionID)",
                        parameters
                    );
                }

                MessageBox.Show("Ваши ответы успешно сохранены!", 
                              "Успех", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении ответов: {ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void NavigateToResults()
        {
            // Здесь будет код перехода к странице результатов
            // NavigationService?.Navigate(new ResultsPage(_userAnswers));
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Models.AnswerOption option)
            {
                // Сбрасываем выбор для всех вариантов в текущем вопросе
                var viewModel = DataContext as SurveyViewModel;
                if (viewModel?.CurrentQuestion != null)
                {
                    foreach (var opt in viewModel.CurrentQuestion.Options)
                    {
                        opt.IsSelected = false;
                    }
                }

                // Устанавливаем выбранный вариант
                option.IsSelected = true;
            }
        }
    }

    public class Question
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string ImagePath { get; set; }
        public int OrderNumber { get; set; }
    }

    public class AnswerOption
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string OptionText { get; set; }
        public string ImagePath { get; set; }
        public int Weight { get; set; }
    }
} 