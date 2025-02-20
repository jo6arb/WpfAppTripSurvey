using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using WpfAppTrip.Models;
using WpfAppTrip.Db;
using System.Collections.Generic;

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
                var hashedPassword = HashPassword(password);
                var parameters = new Dictionary<string, object>
                {
                    { "@Email", email },
                    { "@Password", hashedPassword }
                };

                _currentUser = await _db.GetSingleAsync<User>(
                    "SELECT UserID, Username, Email, Role, RegistrationDate FROM Users " +
                    "WHERE Email = @Email AND Password = @Password",
                    reader => new User
                    {
                        UserID = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        Role = reader.GetString(3),
                        RegistrationDate = reader.GetDateTime(4)
                    },
                    parameters
                );

                return _currentUser != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string Error)> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Сначала проверяем, существует ли пользователь
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
                    return (false, "Пользователь с таким email или телефоном уже существует");
                }

                // Если пользователь не существует, регистрируем
                var hashedPassword = HashPassword(password);
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