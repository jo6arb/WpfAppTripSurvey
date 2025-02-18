using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Db;

namespace WpfAppTrip.Views.Pages
{
    public partial class AdminPage : Page
    {
        private readonly Dbhelper _db;
        public ObservableCollection<UserModel> Users { get; set; }

        public AdminPage()
        {
            InitializeComponent();
            _db = new Dbhelper();
            Users = new ObservableCollection<UserModel>();
            DataContext = this;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await _db.GetDataListAsync<UserModel>(
                    "SELECT * FROM Users",
                    reader => new UserModel
                    {
                        UserID = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(3),
                        Role = reader.GetString(4),
                        RegistrationDate = reader.GetDateTime(5)
                    }
                );

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void SaveUser(UserModel user)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@UserID", user.UserID },
                    { "@Username", user.Username },
                    { "@Email", user.Email },
                    { "@Role", user.Role }
                };

                await _db.ExecuteNonQueryAsync(
                    "UPDATE Users SET Username = @Username, Email = @Email, Role = @Role WHERE UserID = @UserID",
                    parameters
                );

                MessageBox.Show("Пользователь успешно обновлен",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void DeleteUser(UserModel user)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?",
                              "Подтверждение",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@UserID", user.UserID }
                    };

                    await _db.ExecuteNonQueryAsync(
                        "DELETE FROM Users WHERE UserID = @UserID",
                        parameters
                    );

                    Users.Remove(user);
                    MessageBox.Show("Пользователь успешно удален",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
    }

    public class UserModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
} 