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

    // Конструктор для продакшена (принимает строку подключения)
    public SqlTourRepository(string connectionString)
    {
        _connectionString = connectionString;
        _existingConnection = null;
    }

    // Конструктор для тестов (принимает готовое открытое соединение)
    public SqlTourRepository(SqliteConnection connection)
    {
        _existingConnection = connection;
        _connectionString = null;
    }

    // Вспомогательный метод: возвращает соединение и флаг, нужно ли его закрывать
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
            // 🔥 Обновлённый запрос под новую структуру
            command.CommandText = "SELECT * FROM Tours";

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
            command.CommandText = "SELECT * FROM Tours WHERE Id = @Id";
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
            // 🔥 Обновлённый INSERT под новую структуру
            command.CommandText = @"
                INSERT INTO Tours (RouteId, Name, TourType, Destination, StartDate, EndDate, Price, MaxSeats, Description)
                VALUES (@RouteId, @Name, @TourType, @Destination, @StartDate, @EndDate, @Price, @MaxSeats, @Description);
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
            // 🔥 Обновлённый UPDATE под новую структуру
            command.CommandText = @"
                UPDATE Tours SET 
                    RouteId = @RouteId,
                    Name = @Name, 
                    TourType = @TourType,
                    Destination = @Destination,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Price = @Price, 
                    MaxSeats = @MaxSeats,
                    Description = @Description
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
            // 🔥 Жёсткое удаление (так как нет IsActive)
            command.CommandText = "DELETE FROM Tours WHERE Id = @Id";
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

    // 🔥 Маппинг SqliteDataReader → Tour (ОБНОВЛЁННЫЙ)
    private Tour MapTour(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        RouteId = reader.GetInt32(reader.GetOrdinal("RouteId")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        TourType = reader.GetString(reader.GetOrdinal("TourType")),
        Destination = reader.IsDBNull(reader.GetOrdinal("Destination"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("Destination")),
        StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
        EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
        MaxSeats = reader.GetInt32(reader.GetOrdinal("MaxSeats")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("Description"))
    };

    // 🔥 Добавление параметров в команду (ОБНОВЛЁННОЕ)
    private void AddTourParameters(SqliteCommand command, Tour tour)
    {
        command.Parameters.AddWithValue("@RouteId", tour.RouteId);
        command.Parameters.AddWithValue("@Name", tour.Name);
        command.Parameters.AddWithValue("@TourType", tour.TourType);
        command.Parameters.AddWithValue("@Destination", (object?)tour.Destination ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartDate", tour.StartDate);
        command.Parameters.AddWithValue("@EndDate", tour.EndDate);
        command.Parameters.AddWithValue("@Price", tour.Price);
        command.Parameters.AddWithValue("@MaxSeats", tour.MaxSeats);
        command.Parameters.AddWithValue("@Description", (object?)tour.Description ?? DBNull.Value);
    }
}