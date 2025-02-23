using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Services;
using WpfAppTrip.Models;
using WpfAppTrip.Db;
using System.Linq;

namespace WpfAppTrip.ViewModels
{
    public class SurveyViewModel : BaseViewModel
    {
        private readonly Dbhelper _db;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private ObservableCollection<Question> _questions;
        private ObservableCollection<AnswerOption> _currentAnswerOptions;
        private int _currentQuestionIndex;
        private Dictionary<int, int> _userAnswers;
        private bool _isAnswerSelected;

        public SurveyViewModel(
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _db = new Dbhelper();
            _questions = new ObservableCollection<Question>();
            _currentAnswerOptions = new ObservableCollection<AnswerOption>();
            _userAnswers = new Dictionary<int, int>();

            NextCommand = new RelayCommand(_ => NextQuestion(), _ => IsAnswerSelected);
            PreviousCommand = new RelayCommand(_ => PreviousQuestion(), _ => CanGoBack);

            _ = LoadQuestionsAsync();
        }

        public string CurrentQuestionText => $"Вопрос {_currentQuestionIndex + 1} из {_questions?.Count ?? 0}";
        public Question CurrentQuestion => _questions?.ElementAtOrDefault(_currentQuestionIndex);
        public ObservableCollection<AnswerOption> CurrentAnswerOptions => _currentAnswerOptions;
        
        public bool IsAnswerSelected
        {
            get => _isAnswerSelected;
            set => SetProperty(ref _isAnswerSelected, value);
        }

        public bool CanGoBack => _currentQuestionIndex > 0;
        public string NextButtonText => _currentQuestionIndex == (_questions?.Count - 1) ? "Завершить" : "Далее";

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        private async Task LoadQuestionsAsync()
        {
            try
            {
                var questions = await _db.GetDataListAsync<Question>(
                    "SELECT * FROM Questions ORDER BY OrderNumber",
                    reader => new Question
                    {
                        QuestionID = reader.GetInt32(0),
                        QuestionText = reader.GetString(1),
                        ImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                        OrderNumber = reader.GetInt32(3)
                    }
                );

                _questions = new ObservableCollection<Question>(questions);
                if (_questions.Any())
                {
                    await LoadAnswerOptionsForQuestionAsync(_questions[0].QuestionID);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при загрузке вопросов: {ex.Message}");
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

                var options = await _db.GetDataListAsync<AnswerOption>(
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

                _currentAnswerOptions.Clear();
                foreach (var option in options)
                {
                    _currentAnswerOptions.Add(option);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при загрузке вариантов ответа: {ex.Message}");
            }
        }

        public void SelectAnswer(int optionId)
        {
            _userAnswers[CurrentQuestion.QuestionID] = optionId;
            IsAnswerSelected = true;
        }

        private async void NextQuestion()
        {
            if (_currentQuestionIndex < _questions.Count - 1)
            {
                _currentQuestionIndex++;
                await LoadAnswerOptionsForQuestionAsync(_questions[_currentQuestionIndex].QuestionID);
                IsAnswerSelected = _userAnswers.ContainsKey(_questions[_currentQuestionIndex].QuestionID);
                OnPropertyChanged(nameof(CurrentQuestionText));
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(NextButtonText));
                OnPropertyChanged(nameof(CanGoBack));
            }
            else
            {
                await SaveSurveyResultsAsync();
                NavigateToResults();
            }
        }

        private async void PreviousQuestion()
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                await LoadAnswerOptionsForQuestionAsync(_questions[_currentQuestionIndex].QuestionID);
                IsAnswerSelected = _userAnswers.ContainsKey(_questions[_currentQuestionIndex].QuestionID);
                OnPropertyChanged(nameof(CurrentQuestionText));
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(NextButtonText));
                OnPropertyChanged(nameof(CanGoBack));
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
                        { "@UserID", AuthService.CurrentUser.UserID },
                        { "@QuestionID", answer.Key },
                        { "@SelectedOptionID", answer.Value }
                    };

                    await _db.ExecuteNonQueryAsync(
                        "INSERT INTO UserAnswers (UserID, QuestionID, SelectedOptionID) VALUES (@UserID, @QuestionID, @SelectedOptionID)",
                        parameters
                    );
                }

                _dialogService.ShowInfo("Ваши ответы успешно сохранены!");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при сохранении ответов: {ex.Message}");
            }
        }

        private void NavigateToResults()
        {
            // TODO: Реализовать переход к результатам
            _navigationService.NavigateToPage("Results");
        }
    }
} 