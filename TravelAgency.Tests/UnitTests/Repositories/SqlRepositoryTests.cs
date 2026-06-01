using System.Data;
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;

namespace TravelAgency.Data.Repositories;

public class SqlTourRepository : IRepository<Tour>
{
    private readonly string? _connectionString;
    private readonly SqliteConnection? _existingConnection;

    // ✅ Конструктор для продакшена (принимает строку подключения)
    public SqlTourRepository(string connectionString)
    {
        _connectionString = connectionString;
        _existingConnection = null;
    }

    // ✅ Конструктор для тестов (принимает готовое открытое соединение)
    public SqlTourRepository(SqliteConnection connection)
    {
        _existingConnection = connection;
        _connectionString = null;
    }

    // 🔥 Вспомогательный метод: возвращает соединение и флаг, нужно ли его закрывать
    private (SqliteConnection Connection, bool ShouldDispose) GetConnection()
    {
        if (_existingConnection != null && _existingConnection.State == ConnectionState.Open)
        {
            return (_existingConnection, false); // Не закрываем чужое соединение
        }

        if (!string.IsNullOrEmpty(_connectionString))
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return (conn, true); // Закрываем своё соединение
        }

        throw new InvalidOperationException("No database connection available");
    }

    public async Task<IEnumerable<Tour>> GetAllAsync()
    {
        var tours = new List<Tour>();
        var (connection, shouldDispose) = GetConnection();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tours WHERE IsActive = 1";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tours.Add(MapTour(reader));
            }
            return tours;
        }
        finally
        {
            if (shouldDispose)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task<Tour?> GetByIdAsync(int id)
    {
        var (connection, shouldDispose) = GetConnection();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tours WHERE Id = @Id AND IsActive = 1";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTour(reader) : null;
        }
        finally
        {
            if (shouldDispose)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task<Tour> AddAsync(Tour entity)
    {
        var (connection, shouldDispose) = GetConnection();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Tours (Name, Description, Country, City, DurationDays, Price, StartDate, AvailableSeats, IsActive)
                VALUES (@Name, @Description, @Country, @City, @DurationDays, @Price, @StartDate, @AvailableSeats, @IsActive);
                SELECT last_insert_rowid();";

            AddTourParameters(command, entity);
            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            entity.Id = id;
            return entity;
        }
        finally
        {
            if (shouldDispose)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task UpdateAsync(Tour entity)
    {
        var (connection, shouldDispose) = GetConnection();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Tours SET 
                    Name = @Name, 
                    Description = @Description, 
                    Country = @Country, 
                    City = @City, 
                    DurationDays = @DurationDays, 
                    Price = @Price, 
                    StartDate = @StartDate, 
                    AvailableSeats = @AvailableSeats, 
                    IsActive = @IsActive 
                WHERE Id = @Id";

            AddTourParameters(command, entity);
            command.Parameters.AddWithValue("@Id", entity.Id);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (shouldDispose)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task DeleteAsync(int id)
    {
        var (connection, shouldDispose) = GetConnection();

        try
        {
            using var command = connection.CreateCommand();
            // Мягкое удаление: меняем IsActive на 0
            command.CommandText = "UPDATE Tours SET IsActive = 0 WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (shouldDispose)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task<IEnumerable<Tour>> FindAsync(Expression<Func<Tour, bool>> predicate)
    {
        // Получаем все активные туры и фильтруем в памяти (упрощённая реализация)
        var all = await GetAllAsync();
        return all.Where(predicate.Compile());
    }

    // 🔥 Маппинг SqlDataReader → Tour
    private Tour MapTour(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("Description")),
        Country = reader.GetString(reader.GetOrdinal("Country")),
        City = reader.GetString(reader.GetOrdinal("City")),
        DurationDays = reader.GetInt32(reader.GetOrdinal("DurationDays")),
        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
        StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
        AvailableSeats = reader.GetInt32(reader.GetOrdinal("AvailableSeats")),
        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
    };

    // 🔥 Добавление параметров в команду
    private void AddTourParameters(SqliteCommand command, Tour tour)
    {
        command.Parameters.AddWithValue("@Name", tour.Name);
        command.Parameters.AddWithValue("@Description", (object?)tour.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Country", tour.Country);
        command.Parameters.AddWithValue("@City", tour.City);
        command.Parameters.AddWithValue("@DurationDays", tour.DurationDays);
        command.Parameters.AddWithValue("@Price", tour.Price);
        command.Parameters.AddWithValue("@StartDate", tour.StartDate);
        command.Parameters.AddWithValue("@AvailableSeats", tour.AvailableSeats);
        command.Parameters.AddWithValue("@IsActive", tour.IsActive);
    }
}