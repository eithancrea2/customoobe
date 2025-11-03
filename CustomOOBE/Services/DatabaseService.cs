using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using CustomOOBE.Models;

namespace CustomOOBE.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService(string databasePath)
        {
            _databasePath = databasePath;
            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                var directory = Path.GetDirectoryName(_databasePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(_databasePath))
                {
                    SQLiteConnection.CreateFile(_databasePath);
                }

                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                var createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Reviews (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT NOT NULL,
                        Rating INTEGER NOT NULL,
                        Comment TEXT,
                        ComputerName TEXT,
                        Username TEXT
                    )";

                using var command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar base de datos: {ex.Message}");
            }
        }

        public async Task<bool> SaveReviewAsync(Review review)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SQLiteConnection(_connectionString);
                    connection.Open();

                    var insertQuery = @"
                        INSERT INTO Reviews (Date, Rating, Comment, ComputerName, Username)
                        VALUES (@Date, @Rating, @Comment, @ComputerName, @Username)";

                    using var command = new SQLiteCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@Date", review.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@Rating", review.Rating);
                    command.Parameters.AddWithValue("@Comment", review.Comment ?? "");
                    command.Parameters.AddWithValue("@ComputerName", review.ComputerName ?? "");
                    command.Parameters.AddWithValue("@Username", review.Username ?? "");

                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al guardar reseña: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            return await Task.Run(() =>
            {
                var reviews = new List<Review>();

                try
                {
                    using var connection = new SQLiteConnection(_connectionString);
                    connection.Open();

                    var selectQuery = "SELECT * FROM Reviews ORDER BY Date DESC";

                    using var command = new SQLiteCommand(selectQuery, connection);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        reviews.Add(new Review
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.Parse(reader.GetString(1)),
                            Rating = reader.GetInt32(2),
                            Comment = reader.GetString(3),
                            ComputerName = reader.GetString(4),
                            Username = reader.GetString(5)
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al obtener reseñas: {ex.Message}");
                }

                return reviews;
            });
        }

        public async Task<double> GetAverageRatingAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SQLiteConnection(_connectionString);
                    connection.Open();

                    var query = "SELECT AVG(Rating) FROM Reviews";

                    using var command = new SQLiteCommand(query, connection);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDouble(result);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al calcular promedio: {ex.Message}");
                }

                return 0.0;
            });
        }

        public async Task<int> GetTotalReviewsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SQLiteConnection(_connectionString);
                    connection.Open();

                    var query = "SELECT COUNT(*) FROM Reviews";

                    using var command = new SQLiteCommand(query, connection);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al contar reseñas: {ex.Message}");
                }

                return 0;
            });
        }
    }
}
