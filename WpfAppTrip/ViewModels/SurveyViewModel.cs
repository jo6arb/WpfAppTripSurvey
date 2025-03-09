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
using System.Diagnostics;
using WpfAppTrip.Helpers;
using System.IO;

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
            SelectAnswerCommand = new RelayCommand(param => SelectAnswer((int)param));

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
        public ICommand SelectAnswerCommand { get; }

        private async Task LoadQuestionsAsync()
        {
            try
            {
                Debug.WriteLine("Начинаем загрузку вопросов...");
                
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

                Debug.WriteLine($"Загружено вопросов: {questions.Count}");
                
                _questions = new ObservableCollection<Question>(questions);
                if (_questions.Any())
                {
                    Debug.WriteLine($"Загружаем варианты ответов для вопроса ID={_questions[0].QuestionID}");
                    await LoadAnswerOptionsForQuestionAsync(_questions[0].QuestionID);
                    
                    // Принудительно обновим свойства после загрузки данных
                    OnPropertyChanged(nameof(CurrentQuestionText));
                    OnPropertyChanged(nameof(CurrentQuestion));
                    OnPropertyChanged(nameof(CurrentAnswerOptions));
                }
                else
                {
                    Debug.WriteLine("Не найдено ни одного вопроса в базе данных!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке вопросов: {ex.Message}");
                _dialogService.ShowError($"Ошибка при загрузке вопросов: {ex.Message}");
            }
        }

        private async Task LoadAnswerOptionsForQuestionAsync(int questionId)
        {
            try
            {
                Debug.WriteLine($"Загрузка вариантов ответа для вопроса {questionId}");
                
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
                        Weight = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        HasImage = false // Изначально считаем, что изображения нет
                    },
                    parameters
                );

                _currentAnswerOptions.Clear();
                foreach (var option in options)
                {
                    Debug.WriteLine($"Обработка варианта ответа: ID={option.OptionID}, Text={option.OptionText}, DBPath={option.ImagePath}");
                    
                    // Проверяем путь из базы данных
                    bool hasImage = false;
                    string finalImagePath = null;
                    
                    if (!string.IsNullOrEmpty(option.ImagePath))
                    {
                        // Проверяем абсолютный путь
                        if (File.Exists(option.ImagePath))
                        {
                            hasImage = true;
                            finalImagePath = option.ImagePath;
                            Debug.WriteLine($"Найден файл по абсолютному пути: {finalImagePath}");
                        }
                        else
                        {
                            // Проверяем относительно папки проекта
                            string projectPath = Path.Combine(
                                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..")), 
                                option.ImagePath);
                            
                            if (File.Exists(projectPath))
                            {
                                hasImage = true;
                                finalImagePath = projectPath;
                                Debug.WriteLine($"Найден файл относительно проекта: {finalImagePath}");
                            }
                            else
                            {
                                // Проверяем относительно bin/Debug
                                string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, option.ImagePath);
                                
                                if (File.Exists(binPath))
                                {
                                    hasImage = true;
                                    finalImagePath = binPath;
                                    Debug.WriteLine($"Найден файл относительно bin/Debug: {finalImagePath}");
                                }
                            }
                        }
                    }
                    
                    // Если не нашли по пути из БД, ищем по ID вопроса и варианта
                    if (!hasImage)
                    {
                        string foundPath = ImagePathHelper.FindImageFile(questionId, option.OptionID);
                        if (!string.IsNullOrEmpty(foundPath))
                        {
                            hasImage = true;
                            finalImagePath = foundPath;
                            Debug.WriteLine($"Найден файл по ID вопроса и варианта: {finalImagePath}");
                        }
                    }
                    
                    option.HasImage = hasImage;
                    option.ImagePath = finalImagePath;
                    
                    Debug.WriteLine($"Итоговый вариант ответа: ID={option.OptionID}, Text={option.OptionText}, HasImage={option.HasImage}, Path={option.ImagePath}");
                    _currentAnswerOptions.Add(option);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке вариантов ответа: {ex.Message}");
                _dialogService.ShowError($"Ошибка при загрузке вариантов ответа: {ex.Message}");
            }
        }

        private string FormatImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            
            try
            {
                // Проверяем, существует ли файл
                if (ImagePathHelper.IsImageExists(path))
                {
                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                    Debug.WriteLine($"Файл найден: {fullPath}");
                    return fullPath;
                }
                
                Debug.WriteLine($"Файл не найден: {path}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при форматировании пути к изображению: {ex.Message}");
                return null;
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