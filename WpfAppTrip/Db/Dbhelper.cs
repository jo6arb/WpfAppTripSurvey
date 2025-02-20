using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;
using System.Diagnostics;

namespace WpfAppTrip.Db
{
    public class Dbhelper
    {
        private readonly string _connectionString;

        public Dbhelper()
        {
            _connectionString = (string)Application.Current.Resources["ConnectionString"];
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
    }
}
