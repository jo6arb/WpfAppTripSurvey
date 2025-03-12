using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WpfAppTrip.Db;
using WpfAppTrip.Models;

namespace WpfAppTrip.Services
{
    public class AuthService
    {
        private readonly Dbhelper _dbHelper;
        private User _currentUser;

        public AuthService()
        {
            _dbHelper = new Dbhelper();
        }

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public bool IsAdmin => _currentUser?.Role == "Admin";

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // Хеширование пароля (в реальном приложении)
                string hashedPassword = HashPassword(password);
                
                // В учебном примере можно использовать простое сравнение
                var parameters = new Dictionary<string, object>
                {
                    { "@Username", username },
                    { "@Password", password } // В реальном приложении здесь должен быть hashedPassword
                };

                var user = await _dbHelper.GetDataAsync<User>(
                    "SELECT * FROM Users WHERE Username = @Username AND Password = @Password",
                    parameters,
                    reader => new User
                    {
                        UserID = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(3),
                        Role = reader.GetString(4)
                    });

                if (user != null)
                {
                    _currentUser = user;
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string email)
        {
            try
            {
                // Проверка, существует ли пользователь
                var checkParams = new Dictionary<string, object>
                {
                    { "@Username", username }
                };

                var existingUser = await _dbHelper.GetDataAsync<User>(
                    "SELECT * FROM Users WHERE Username = @Username",
                    checkParams,
                    reader => new User { UserID = reader.GetInt32(0) });

                if (existingUser != null)
                {
                    return false; // Пользователь уже существует
                }

                // Хеширование пароля (в реальном приложении)
                string hashedPassword = HashPassword(password);

                // Добавление нового пользователя
                var parameters = new Dictionary<string, object>
                {
                    { "@Username", username },
                    { "@Password", password }, // В реальном приложении здесь должен быть hashedPassword
                    { "@Email", email },
                    { "@Role", "User" }
                };

                int result = await _dbHelper.ExecuteNonQueryAsync(
                    "INSERT INTO Users (Username, Password, Email, Role) VALUES (@Username, @Password, @Email, @Role)",
                    parameters);

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }

        private string HashPassword(string password)
        {
            // Простая реализация хеширования пароля
            // В реальном приложении следует использовать более надежные методы
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 