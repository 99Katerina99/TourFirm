using Microsoft.EntityFrameworkCore;
using TravelAgency.Core.Entities;

namespace TravelAgency.Data.DbContext;

public class TravelAgencyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    private readonly string? _connectionString;

    // ✅ Конструктор для DI и тестов (принимает настроенные опции)
    public TravelAgencyDbContext(DbContextOptions<TravelAgencyDbContext> options)
        : base(options) { }

    // ✅ Конструктор для серверного приложения (принимает строку подключения)
    public TravelAgencyDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Применяем строку подключения только если опции не были переданы через конструктор/DI
        if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString))
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === User ===
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Role).HasDefaultValue("user");
        });

        // === Tour ===
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(t => t.Price).HasPrecision(10, 2);
            entity.HasIndex(t => t.Country);
            entity.HasIndex(t => t.IsActive);
        });

        // === Client ===
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(c => c.Phone);
        });

        // === Booking ===
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(b => b.TotalPrice).HasPrecision(10, 2);
            entity.HasIndex(b => b.Status);

            // Внешние ключи
            entity.HasOne(b => b.Client).WithMany().HasForeignKey(b => b.ClientId);
            entity.HasOne(b => b.Tour).WithMany().HasForeignKey(b => b.TourId);
            entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId);
        });
    }
}