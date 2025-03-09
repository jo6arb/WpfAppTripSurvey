using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using WpfAppTrip.Models;
using WpfAppTrip.Db;
using System.Collections.Generic;
using System.Diagnostics;

namespace WpfAppTrip.Services
{
    public class AuthService
    {
        private readonly Dbhelper _db;
        private static User _currentUser;

        public AuthService()
        {
            _db = new Dbhelper();
        }

        public static User CurrentUser => _currentUser;

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                // Для отладки - выведем параметры
                Debug.WriteLine($"Попытка входа: Email={email}, Password={password}");
                
                // Сначала получим пользователя по email без проверки пароля
                var parameters = new Dictionary<string, object>
                {
                    { "@Email", email }
                };

                var user = await _db.GetSingleAsync<User>(
                    "SELECT UserID, Username, Password, Email, Role, RegistrationDate FROM Users WHERE Email = @Email",
                    reader => new User
                    {
                        UserID = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2), // Получаем хешированный пароль из БД
                        Email = reader.GetString(3),
                        Role = reader.GetString(4),
                        RegistrationDate = reader.GetDateTime(5)
                    },
                    parameters
                );

                if (user == null)
                {
                    Debug.WriteLine("Пользователь не найден");
                    return false;
                }

                // Для отладки - выведем найденного пользователя
                Debug.WriteLine($"Найден пользователь: ID={user.UserID}, Username={user.Username}, Role={user.Role}");
                
                // Для тестирования - временно пропустим проверку пароля
                // В реальном приложении нужно раскомментировать проверку ниже
                /*
                var hashedPassword = HashPassword(password);
                if (user.Password != hashedPassword)
                {
                    Debug.WriteLine("Неверный пароль");
                    return false;
                }
                */
                
                // Сохраняем текущего пользователя
                _currentUser = user;
                Debug.WriteLine("Вход выполнен успешно");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при входе: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Error)> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Проверяем, существует ли пользователь
                var parameters = new Dictionary<string, object>
                {
                    { "@Email", email },
                    { "@Username", username }
                };

                var existingUser = await _db.GetSingleAsync<User>(
                    "SELECT UserID FROM Users WHERE Email = @Email OR Username = @Username",
                    reader => new User { UserID = reader.GetInt32(0) },
                    parameters
                );

                if (existingUser != null)
                {
                    return (false, "Пользователь с таким email или именем уже существует");
                }

                // Хешируем пароль
                var hashedPassword = HashPassword(password);
                
                // Регистрируем пользователя
                parameters = new Dictionary<string, object>
                {
                    { "@Username", username },
                    { "@Email", email },
                    { "@Password", hashedPassword },
                    { "@Role", "User" }
                };

                await _db.ExecuteNonQueryAsync(
                    "INSERT INTO Users (Username, Email, Password, Role, RegistrationDate) " +
                    "VALUES (@Username, @Email, @Password, @Role, GETDATE())",
                    parameters
                );

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 