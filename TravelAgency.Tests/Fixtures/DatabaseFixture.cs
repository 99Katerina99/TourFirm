using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// 🔥 ВАЖНО: Используем алиас, чтобы избежать конфликта имён с Microsoft.EntityFrameworkCore.DbContext
using AppDb = TravelAgency.Data.DbContext;

namespace TravelAgency.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    // Держим соединение открытым на всё время жизни фикстуры (критично для in-memory SQLite)
    protected readonly SqliteConnection _connection;

    public DatabaseFixture()
    {
        // 1. Создаём и открываем in-memory соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // 2. Настраиваем EF Core использовать ЭТО ЖЕ соединение (не строку!)
        var options = new DbContextOptionsBuilder<AppDb.TravelAgencyDbContext>()
            .UseSqlite(_connection)  // ← Передаём объект соединения
            .Options;

        // 3. Создаём контекст с этими опциями (используем алиас)
        DbContext = new AppDb.TravelAgencyDbContext(options);

        // 4. Создаём таблицы (теперь они будут видны и в EF Core, и в raw SQL)
        CreateTables();
    }

    public AppDb.TravelAgencyDbContext DbContext { get; }

    private void CreateTables()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                Role TEXT DEFAULT 'user',
                CreatedAt TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS Tours (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Country TEXT NOT NULL,
                City TEXT NOT NULL,
                DurationDays INTEGER NOT NULL,
                Price REAL NOT NULL,
                StartDate TEXT NOT NULL,
                AvailableSeats INTEGER NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
            
            CREATE TABLE IF NOT EXISTS Clients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT NOT NULL,
                Email TEXT,
                PassportData TEXT,
                CreatedAt TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS Bookings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientId INTEGER NOT NULL,
                TourId INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                BookingDate TEXT NOT NULL,
                PersonsCount INTEGER NOT NULL DEFAULT 1,
                TotalPrice REAL NOT NULL,
                Status TEXT NOT NULL DEFAULT 'pending',
                FOREIGN KEY (ClientId) REFERENCES Clients(Id),
                FOREIGN KEY (TourId) REFERENCES Tours(Id),
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
        ";
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}