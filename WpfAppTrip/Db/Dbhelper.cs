using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using WpfAppTrip.Models;

namespace WpfAppTrip.Db
{
    public class Dbhelper
    {
        private readonly string _connectionString;
        
        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        public User CurrentUser { get; set; }

        public Dbhelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        /// <summary>
        /// Выполняет SQL запрос без возврата данных
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        
                        Debug.WriteLine($"Executing query: {query}");
                        foreach (var param in parameters ?? new Dictionary<string, object>())
                        {
                            Debug.WriteLine($"Parameter {param.Key}: {param.Value}");
                        }
                        
                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Выполняет SQL запрос и возвращает скалярное значение
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    return result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }

        /// <summary>
        /// Выполняет SQL запрос и возвращает DataTable
        /// </summary>
        public async Task<DataTable> GetDataTableAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    await connection.OpenAsync();
                    var dataTable = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dataTable));
                    }
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Выполняет SQL запрос и возвращает список объектов через маппер
        /// </summary>
        public async Task<List<T>> GetDataListAsync<T>(string query, Func<IDataRecord, T> mapper, Dictionary<string, object> parameters = null)
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(mapper(reader));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Выполняет SQL запрос и возвращает один объект через маппер
        /// </summary>
        public async Task<T> GetSingleAsync<T>(string query, Func<IDataRecord, T> mapper, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return mapper(reader);
                        }
                        return default(T);
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет хранимую процедуру
        /// </summary>
        public async Task<DataTable> ExecuteStoredProcedureAsync(string procName, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(procName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    await connection.OpenAsync();
                    var dataTable = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dataTable));
                    }
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Начинает транзакцию
        /// </summary>
        public async Task<SqlTransaction> BeginTransactionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection.BeginTransaction();
        }

        /// <summary>
        /// Выполняет SQL запрос в рамках транзакции
        /// </summary>
        public async Task ExecuteTransactionAsync(SqlTransaction transaction, string query, Dictionary<string, object> parameters = null)
        {
            using (var command = new SqlCommand(query, transaction.Connection, transaction))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Сохраняет информацию о билете в базу данных
        /// </summary>
        /// <param name="ticket">Данные билета</param>
        /// <returns>ID созданной записи или -1 в случае ошибки</returns>
        public int SaveTicket(TicketData ticket)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string sql = @"
                        INSERT INTO Tickets (
                            PassengerLastName, PassengerFirstName, PassengerGender, PassengerBirthDate,
                            DocumentType, DocumentNumber, DocumentExpiryDate,
                            BuyerLastName, BuyerFirstName, BuyerEmail, BuyerPhone,
                            RouteInfo, ClassInfo, TotalPrice, TicketNumber, TicketFilePath, PurchaseDate,
                            UserID, TourID
                        ) VALUES (
                            @PassengerLastName, @PassengerFirstName, @PassengerGender, @PassengerBirthDate,
                            @DocumentType, @DocumentNumber, @DocumentExpiryDate,
                            @BuyerLastName, @BuyerFirstName, @BuyerEmail, @BuyerPhone,
                            @RouteInfo, @ClassInfo, @TotalPrice, @TicketNumber, @TicketFilePath, @PurchaseDate,
                            @UserID, @TourID
                        );
                        SELECT SCOPE_IDENTITY();";
                    
                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Информация о пассажире
                        command.Parameters.AddWithValue("@PassengerLastName", ticket.PassengerLastName);
                        command.Parameters.AddWithValue("@PassengerFirstName", ticket.PassengerFirstName);
                        command.Parameters.AddWithValue("@PassengerGender", ticket.PassengerGender ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PassengerBirthDate", ticket.PassengerBirthDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DocumentType", ticket.DocumentType ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DocumentNumber", ticket.DocumentNumber);
                        command.Parameters.AddWithValue("@DocumentExpiryDate", ticket.DocumentExpiryDate ?? (object)DBNull.Value);
                        
                        // Информация о покупателе
                        command.Parameters.AddWithValue("@BuyerLastName", ticket.BuyerLastName);
                        command.Parameters.AddWithValue("@BuyerFirstName", ticket.BuyerFirstName);
                        command.Parameters.AddWithValue("@BuyerEmail", ticket.BuyerEmail ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@BuyerPhone", ticket.BuyerPhone ?? (object)DBNull.Value);
                        
                        // Информация о маршруте
                        command.Parameters.AddWithValue("@RouteInfo", ticket.RouteInfo);
                        command.Parameters.AddWithValue("@ClassInfo", ticket.ClassInfo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TotalPrice", ticket.TotalPrice);
                        
                        // Информация о билете
                        command.Parameters.AddWithValue("@TicketNumber", ticket.TicketNumber);
                        command.Parameters.AddWithValue("@TicketFilePath", ticket.TicketFilePath ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PurchaseDate", ticket.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        
                        // Связи
                        command.Parameters.AddWithValue("@UserID", CurrentUser?.UserID ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TourID", (object)DBNull.Value); // Можно добавить TourID, если он доступен
                        
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении билета: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Получает список билетов пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список билетов</returns>
        public List<TicketData> GetUserTickets(int userId)
        {
            var tickets = new List<TicketData>();
            
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    string sql = @"
                        SELECT * FROM Tickets
                        WHERE UserID = @UserID
                        ORDER BY TicketID DESC";
                    
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        
                        Debug.WriteLine($"Выполняется запрос билетов для пользователя с ID: {userId}");
                        
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                var ticket = new TicketData
                                {
                                    // Информация о пассажире
                                    PassengerLastName = reader["PassengerLastName"].ToString(),
                                    PassengerFirstName = reader["PassengerFirstName"].ToString(),
                                    PassengerGender = reader["PassengerGender"]?.ToString(),
                                    PassengerBirthDate = reader["PassengerBirthDate"] != DBNull.Value 
                                        ? DateTime.Parse(reader["PassengerBirthDate"].ToString()) 
                                        : (DateTime?)null,
                                    DocumentType = reader["DocumentType"]?.ToString(),
                                    DocumentNumber = reader["DocumentNumber"].ToString(),
                                    DocumentExpiryDate = reader["DocumentExpiryDate"]?.ToString(),
                                    
                                    // Информация о покупателе
                                    BuyerLastName = reader["BuyerLastName"].ToString(),
                                    BuyerFirstName = reader["BuyerFirstName"].ToString(),
                                    BuyerEmail = reader["BuyerEmail"]?.ToString(),
                                    BuyerPhone = reader["BuyerPhone"]?.ToString(),
                                    
                                    // Информация о маршруте
                                    RouteInfo = reader["RouteInfo"].ToString(),
                                    ClassInfo = reader["ClassInfo"]?.ToString(),
                                    TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                                    
                                    // Информация о билете
                                    TicketNumber = reader["TicketNumber"].ToString(),
                                    TicketFilePath = reader["TicketFilePath"]?.ToString(),
                                    PurchaseDate = DateTime.Parse(reader["PurchaseDate"].ToString())
                                };
                                
                                tickets.Add(ticket);
                            }
                            Debug.WriteLine($"Загружено {count} билетов для пользователя с ID: {userId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении билетов: {ex.Message}");
            }
            
            return tickets;
        }

        /// <summary>
        /// Получает пользователя по email
        /// </summary>
        public User GetUserByEmail(string email)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        @"SELECT UserID, Username, Email, Password, Role, Phone, 
                          LastName, FirstName, MiddleName, COALESCE(RegistrationDate, GETDATE()) AS RegistrationDate 
                          FROM Users WHERE Email = @Email", connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Password = reader.GetString(reader.GetOrdinal("Password")),
                                    Role = reader.GetString(reader.GetOrdinal("Role")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
                                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? null : reader.GetString(reader.GetOrdinal("FirstName")),
                                    MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? null : reader.GetString(reader.GetOrdinal("MiddleName")),
                                    RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"))
                                };
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении пользователя по email: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Получает пользователя по телефону
        /// </summary>
        public User GetUserByPhone(string phone)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        @"SELECT UserID, Username, Email, Password, Role, Phone, 
                          LastName, FirstName, MiddleName, COALESCE(RegistrationDate, GETDATE()) AS RegistrationDate 
                          FROM Users WHERE Phone = @Phone", connection))
                    {
                        command.Parameters.AddWithValue("@Phone", phone);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Password = reader.GetString(reader.GetOrdinal("Password")),
                                    Role = reader.GetString(reader.GetOrdinal("Role")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
                                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? null : reader.GetString(reader.GetOrdinal("FirstName")),
                                    MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? null : reader.GetString(reader.GetOrdinal("MiddleName")),
                                    RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"))
                                };
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении пользователя по телефону: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Сохраняет информацию о входе пользователя
        /// </summary>
        public void SaveLoginHistory(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Сначала проверяем, существует ли таблица LoginHistory
                    bool tableExists = false;
                    using (var checkCommand = new SqlCommand(
                        @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                          WHERE TABLE_NAME = 'LoginHistory'", connection))
                    {
                        tableExists = (int)checkCommand.ExecuteScalar() > 0;
                    }
                    
                    // Если таблица не существует, создаем её
                    if (!tableExists)
                    {
                        Debug.WriteLine("Таблица LoginHistory не существует. Создание таблицы отложено.");
                        return; // Пропускаем сохранение истории входа
                    }
                    
                    // Получаем имя компьютера и имя пользователя Windows
                    string computerName = Environment.MachineName;
                    string windowsUserName = Environment.UserName;
                    
                    // Если таблица существует, сохраняем историю входа
                    using (var command = new SqlCommand(
                        @"INSERT INTO LoginHistory (UserID, LoginDate, ComputerName, UserWindowsName) 
                          VALUES (@UserID, GETDATE(), @ComputerName, @UserWindowsName)", connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@ComputerName", computerName);
                        command.Parameters.AddWithValue("@UserWindowsName", windowsUserName);
                        
                        command.ExecuteNonQuery();
                        Debug.WriteLine($"Сохранена история входа для пользователя {userId} с компьютера {computerName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении истории входа: {ex.Message}");
                // Не выбрасываем исключение, чтобы не прерывать процесс входа
            }
        }
    }
}
