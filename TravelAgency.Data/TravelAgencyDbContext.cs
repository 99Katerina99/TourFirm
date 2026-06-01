using Microsoft.EntityFrameworkCore;
using TravelAgency.Core.Entities;

public class TravelAgencyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    // ✅ Конструктор для тестов (с DbContextOptions)
    public TravelAgencyDbContext(DbContextOptions<TravelAgencyDbContext> options)
        : base(options) { }

    // ✅ Конструктор для продакшена (со строкой подключения)
    public TravelAgencyDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    private readonly string? _connectionString;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Используем строку подключения только если options не настроен (для прода)
        if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString))
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... ваша конфигурация ...
        base.OnModelCreating(modelBuilder);
    }
}