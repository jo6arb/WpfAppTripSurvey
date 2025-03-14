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

        public bool IsAuthenticated => CurrentUser != null;
        public User CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

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
        /// Выполняет вход пользователя в систему по телефону или email
        /// </summary>
        /// <param name="loginValue">Телефон или email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public Task<(bool Success, string Error)> LoginAsync(string loginValue, string password)
        {
            try
            {
                Debug.WriteLine($"Попытка входа: Phone/Email={loginValue}");
                
                // Проверяем, является ли loginValue телефоном или email
                bool isEmail = loginValue.Contains("@");
                
                // Получаем пользователя по телефону или email
                User user = isEmail 
                    ? _dbHelper.GetUserByEmail(loginValue) 
                    : _dbHelper.GetUserByPhone(loginValue);
                
                if (user == null)
                {
                    Debug.WriteLine("Пользователь не найден");
                    return Task.FromResult((false, "Пользователь не найден"));
                }

                // Проверяем пароль напрямую
                if (user.Password != password)
                {
                    Debug.WriteLine("Неверный пароль");
                    return Task.FromResult((false, "Неверный пароль"));
                }

                // Устанавливаем текущего пользователя
                CurrentUser = user;
                _dbHelper.CurrentUser = user;
                
                // Сохраняем информацию о входе
                _dbHelper.SaveLoginHistory(user.UserID);
                
                Debug.WriteLine($"Успешный вход пользователя: {user.GetFullName()}, Роль: {user.Role}");
                return Task.FromResult((true, string.Empty));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при входе: {ex.Message}");
                return Task.FromResult((false, $"Ошибка при входе: {ex.Message}"));
            }
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <param name="phone">Телефон пользователя</param>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="lastName">Фамилия пользователя</param>
        /// <param name="firstName">Имя пользователя</param>
        /// <param name="middleName">Отчество пользователя</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> RegisterAsync(string phone, string email, string password, 
            string lastName, string firstName, string middleName = null)
        {
            try
            {
                Debug.WriteLine($"Попытка регистрации: Phone={phone}, Email={email}, Name={lastName} {firstName} {middleName}");

                if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
                {
                    return (false, "Все обязательные поля должны быть заполнены");
                }

                // Проверка, существует ли пользователь с таким телефоном
                var checkParams = new Dictionary<string, object>
                {
                    { "@Phone", phone }
                };

                int userCount = await _dbHelper.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Phone = @Phone",
                    checkParams);

                if (userCount > 0)
                {
                    Debug.WriteLine($"Пользователь с телефоном {phone} уже существует");
                    return (false, "Пользователь с таким телефоном уже существует");
                }

                // Проверка, существует ли пользователь с таким email
                checkParams = new Dictionary<string, object>
                {
                    { "@Email", email }
                };

                userCount = await _dbHelper.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email",
                    checkParams);

                if (userCount > 0)
                {
                    Debug.WriteLine($"Пользователь с email {email} уже существует");
                    return (false, "Пользователь с таким email уже существует");
                }

                // Создаем имя пользователя из фамилии и имени
                string username = $"{lastName} {firstName}";
                
                // Сохраняем пароль без хеширования
                var parameters = new Dictionary<string, object>
                {
                    { "@Username", username },
                    { "@Email", email },
                    { "@Password", password },
                    { "@Phone", phone },
                    { "@LastName", lastName },
                    { "@FirstName", firstName },
                    { "@MiddleName", middleName ?? (object)DBNull.Value },
                    { "@RegistrationDate", DateTime.Now }
                };

                // Роль User по умолчанию
                await _dbHelper.ExecuteNonQueryAsync(
                    @"INSERT INTO Users (Username, Email, Password, Phone, LastName, FirstName, MiddleName, Role, RegistrationDate) 
                      VALUES (@Username, @Email, @Password, @Phone, @LastName, @FirstName, @MiddleName, 'User', @RegistrationDate)",
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
            _dbHelper.CurrentUser = null;
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
        /// <param name="email">Новый email</param>
        /// <param name="lastName">Новая фамилия</param>
        /// <param name="firstName">Новое имя</param>
        /// <param name="middleName">Новое отчество</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> UpdateUserInfoAsync(int userId, string email, 
            string lastName, string firstName, string middleName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
                {
                    return (false, "Email, фамилия и имя обязательны для заполнения");
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

                // Создаем имя пользователя из фамилии и имени
                string username = $"{lastName} {firstName}";

                // Обновление информации о пользователе
                var parameters = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Username", username },
                    { "@Email", email },
                    { "@LastName", lastName },
                    { "@FirstName", firstName },
                    { "@MiddleName", middleName ?? (object)DBNull.Value }
                };

                await _dbHelper.ExecuteNonQueryAsync(
                    @"UPDATE Users SET 
                      Username = @Username, 
                      Email = @Email, 
                      LastName = @LastName, 
                      FirstName = @FirstName, 
                      MiddleName = @MiddleName 
                      WHERE UserID = @UserID",
                    parameters);

                // Обновляем текущего пользователя, если это он
                if (_currentUser != null && _currentUser.UserID == userId)
                {
                    _currentUser.Username = username;
                    _currentUser.Email = email;
                    _currentUser.LastName = lastName;
                    _currentUser.FirstName = firstName;
                    _currentUser.MiddleName = middleName;
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

        /// <summary>
        /// Обновляет телефон пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="phone">Новый номер телефона</param>
        /// <returns>Кортеж с результатом операции и сообщением об ошибке</returns>
        public async Task<(bool Success, string Error)> UpdateUserPhoneAsync(int userId, string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    return (false, "Телефон обязателен для заполнения");
                }

                // Проверка, существует ли другой пользователь с таким телефоном
                var checkParams = new Dictionary<string, object>
                {
                    { "@Phone", phone },
                    { "@UserID", userId }
                };

                int userCount = await _dbHelper.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Phone = @Phone AND UserID != @UserID",
                    checkParams);

                if (userCount > 0)
                {
                    return (false, "Пользователь с таким телефоном уже существует");
                }

                // Обновление телефона пользователя
                var parameters = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Phone", phone }
                };

                await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET Phone = @Phone WHERE UserID = @UserID",
                    parameters);

                // Обновляем текущего пользователя, если это он
                if (_currentUser != null && _currentUser.UserID == userId)
                {
                    _currentUser.Phone = phone;
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении телефона пользователя: {ex.Message}");
                return (false, $"Ошибка обновления: {ex.Message}");
            }
        }
    }
} 