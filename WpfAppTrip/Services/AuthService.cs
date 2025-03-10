using System;
using System.Threading.Tasks;
using System.Text;
using WpfAppTrip.Models;
using WpfAppTrip.Db;
using System.Collections.Generic;
using System.Diagnostics;

namespace WpfAppTrip.Services
{
    /// <summary>
    /// Сервис для управления аутентификацией и авторизацией пользователей
    /// </summary>
    public class AuthService
    {
        private readonly Dbhelper _dbHelper;
        private User _currentUser;

        /// <summary>
        /// Инициализирует новый экземпляр класса AuthService с заданным помощником базы данных
        /// </summary>
        /// <param name="dbHelper">Помощник для работы с базой данных</param>
        public AuthService(Dbhelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        /// <summary>
        /// Получает текущего аутентифицированного пользователя
        /// </summary>
        /// <returns>Текущий пользователь или null, если пользователь не аутентифицирован</returns>
        public User GetCurrentUser() => _currentUser;

        /// <summary>
        /// Выполняет вход пользователя в систему
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> LoginAsync(string email, string password)
        {
            try
            {
                Debug.WriteLine($"Попытка входа: Email={email}");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return (false, "Email и пароль обязательны для заполнения");
                }

                var parameters = new Dictionary<string, object>
                {
                    { "@Email", email }
                };

                var user = await _dbHelper.GetSingleAsync<User>(
                    "SELECT UserID, Username, Email, Password, Role, COALESCE(RegistrationDate, GETDATE()) AS RegistrationDate FROM Users WHERE Email = @Email",
                    reader => new User
                    {
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"))
                    },
                    parameters);

                if (user == null)
                {
                    Debug.WriteLine("Пользователь не найден");
                    return (false, "Неверный email или пароль");
                }

                // Простая проверка пароля без хеширования
                if (password != user.Password)
                {
                    Debug.WriteLine("Неверный пароль");
                    return (false, "Неверный email или пароль");
                }

                // Очищаем пароль перед сохранением в памяти
                user.Password = null;
                _currentUser = user;
                
                Debug.WriteLine($"Успешный вход пользователя: {user.Username}, Роль: {user.Role}");
                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при входе: {ex.Message}");
                return (false, $"Ошибка входа: {ex.Message}");
            }
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> RegisterAsync(string username, string email, string password)
        {
            try
            {
                Debug.WriteLine($"Попытка регистрации: Username={username}, Email={email}");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return (false, "Все поля обязательны для заполнения");
                }

                // Проверка, существует ли пользователь с таким email
                var checkParams = new Dictionary<string, object>
                {
                    { "@Email", email }
                };

                int userCount = await _dbHelper.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email",
                    checkParams);

                if (userCount > 0)
                {
                    Debug.WriteLine($"Пользователь с email {email} уже существует");
                    return (false, "Пользователь с таким email уже существует");
                }

                // Сохраняем пароль без хеширования
                var parameters = new Dictionary<string, object>
                {
                    { "@Username", username },
                    { "@Email", email },
                    { "@Password", password }, // Сохраняем пароль как есть
                    { "@RegistrationDate", DateTime.Now }
                };

                // Роль User по умолчанию
                await _dbHelper.ExecuteNonQueryAsync(
                    @"INSERT INTO Users (Username, Email, Password, Role, RegistrationDate) 
                      VALUES (@Username, @Email, @Password, 'User', @RegistrationDate)",
                    parameters);

                Debug.WriteLine($"Пользователь {username} успешно зарегистрирован");
                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при регистрации: {ex.Message}");
                return (false, $"Ошибка регистрации: {ex.Message}");
            }
        }

        /// <summary>
        /// Выход пользователя из системы
        /// </summary>
        public void Logout()
        {
            Debug.WriteLine("Выход пользователя из системы");
            _currentUser = null;
        }

        /// <summary>
        /// Проверяет, является ли текущий пользователь администратором
        /// </summary>
        public bool IsCurrentUserAdmin()
        {
            return _currentUser?.IsAdmin ?? false;
        }

        /// <summary>
        /// Обновляет информацию о пользователе
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="username">Новое имя пользователя</param>
        /// <param name="email">Новый email</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> UpdateUserInfoAsync(int userId, string username, string email)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
                {
                    return (false, "Имя пользователя и email обязательны для заполнения");
                }

                // Проверка, существует ли другой пользователь с таким email
                var checkParams = new Dictionary<string, object>
                {
                    { "@Email", email },
                    { "@UserID", userId }
                };

                int userCount = await _dbHelper.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID != @UserID",
                    checkParams);

                if (userCount > 0)
                {
                    return (false, "Пользователь с таким email уже существует");
                }

                // Обновление информации о пользователе
                var parameters = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Username", username },
                    { "@Email", email }
                };

                await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET Username = @Username, Email = @Email WHERE UserID = @UserID",
                    parameters);

                // Обновляем текущего пользователя, если это он
                if (_currentUser != null && _currentUser.UserID == userId)
                {
                    _currentUser.Username = username;
                    _currentUser.Email = email;
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении информации о пользователе: {ex.Message}");
                return (false, $"Ошибка обновления: {ex.Message}");
            }
        }

        /// <summary>
        /// Изменяет пароль пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="currentPassword">Текущий пароль</param>
        /// <param name="newPassword">Новый пароль</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                {
                    return (false, "Текущий и новый пароли обязательны для заполнения");
                }

                // Получаем текущий пароль из базы данных
                var parameters = new Dictionary<string, object>
                {
                    { "@UserID", userId }
                };

                string storedPassword = await _dbHelper.ExecuteScalarAsync<string>(
                    "SELECT Password FROM Users WHERE UserID = @UserID",
                    parameters);

                if (string.IsNullOrEmpty(storedPassword))
                {
                    return (false, "Пользователь не найден");
                }

                // Проверяем, совпадает ли текущий пароль
                if (currentPassword != storedPassword)
                {
                    return (false, "Текущий пароль указан неверно");
                }

                // Обновляем пароль
                var updateParams = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Password", newPassword } // Сохраняем пароль как есть
                };

                await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET Password = @Password WHERE UserID = @UserID",
                    updateParams);

                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при изменении пароля: {ex.Message}");
                return (false, $"Ошибка изменения пароля: {ex.Message}");
            }
        }
    }
} 