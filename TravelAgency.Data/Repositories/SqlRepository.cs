using System.Data;
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;

namespace TravelAgency.Data.Repositories;

public class SqlTourRepository : IRepository<Tour>
{
    private readonly string _connectionString;

    public SqlTourRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Tour>> GetAllAsync()
    {
        var tours = new List<Tour>();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tours WHERE IsActive = 1";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tours.Add(MapTour(reader));
        }
        return tours;
    }

    public async Task<Tour?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tours WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapTour(reader) : null;
    }

    public async Task<Tour> AddAsync(Tour entity)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

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

    public async Task UpdateAsync(Tour entity)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Tours SET Name=@Name, Description=@Description, Country=@Country, 
            City=@City, DurationDays=@DurationDays, Price=@Price, StartDate=@StartDate, 
            AvailableSeats=@AvailableSeats, IsActive=@IsActive WHERE Id=@Id";

        AddTourParameters(command, entity);
        command.Parameters.AddWithValue("@Id", entity.Id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tours WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Tour>> FindAsync(Expression<Func<Tour, bool>> predicate)
    {
        // Упрощённая реализация: фильтрация в памяти
        var all = await GetAllAsync();
        return all.Where(predicate.Compile());
    }

    private Tour MapTour(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Description = reader.GetString(reader.GetOrdinal("Description")),
        Country = reader.GetString(reader.GetOrdinal("Country")),
        City = reader.GetString(reader.GetOrdinal("City")),
        DurationDays = reader.GetInt32(reader.GetOrdinal("DurationDays")),
        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
        StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
        AvailableSeats = reader.GetInt32(reader.GetOrdinal("AvailableSeats")),
        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
    };

    private void AddTourParameters(SqliteCommand command, Tour tour)
    {
        command.Parameters.AddWithValue("@Name", tour.Name);
        command.Parameters.AddWithValue("@Description", tour.Description);
        command.Parameters.AddWithValue("@Country", tour.Country);
        command.Parameters.AddWithValue("@City", tour.City);
        command.Parameters.AddWithValue("@DurationDays", tour.DurationDays);
        command.Parameters.AddWithValue("@Price", tour.Price);
        command.Parameters.AddWithValue("@StartDate", tour.StartDate);
        command.Parameters.AddWithValue("@AvailableSeats", tour.AvailableSeats);
        command.Parameters.AddWithValue("@IsActive", tour.IsActive);
    }
}