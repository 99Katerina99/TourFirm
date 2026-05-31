using Microsoft.EntityFrameworkCore;
using TravelAgency.Core.Entities;

namespace TravelAgency.Data.DbContext;

public class TravelAgencyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    private readonly string _connectionString;

    public TravelAgencyDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Role).HasDefaultValue("user");
        });

        // Tour configuration
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(t => t.Price).HasPrecision(10, 2);
            entity.HasIndex(t => t.Country);
            entity.HasIndex(t => t.IsActive);
        });

        // Client configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(c => c.Phone);
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(b => b.TotalPrice).HasPrecision(10, 2);
            entity.HasIndex(b => b.Status);
        });
    }
}