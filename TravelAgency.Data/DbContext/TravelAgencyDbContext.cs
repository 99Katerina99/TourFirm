using Microsoft.EntityFrameworkCore;
using TravelAgency.Core.Entities;

namespace TravelAgency.Data.DbContext;

public class TravelAgencyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Route> Routes { get; set; }              // НОВОЕ
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Request> Requests { get; set; }          // НОВОЕ
    public DbSet<ClientContract> ClientContracts { get; set; }  // НОВОЕ
    public DbSet<Payment> Payments { get; set; }          // НОВОЕ
    public DbSet<Booking> Bookings { get; set; }          // НОВОЕ
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierContract> SupplierContracts { get; set; }  // НОВОЕ

    private readonly string? _connectionString;

    //  Конструктор для DI и тестов (принимает настроенные опции)
    public TravelAgencyDbContext(DbContextOptions<TravelAgencyDbContext> options)
        : base(options) { }

    //  Конструктор для серверного приложения (принимает строку подключения)
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

        // Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PassportSeries).HasMaxLength(4);
            entity.Property(e => e.PassportNumber).HasMaxLength(6);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Route
        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        // Tour
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Route)
                  .WithMany(r => r.Tours)
                  .HasForeignKey(e => e.RouteId);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.HasIndex(e => e.TourType);
        });

        // Request
        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Requests)
                  .HasForeignKey(e => e.ClientId);
        });

        // ClientContract
        modelBuilder.Entity<ClientContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalCost).HasPrecision(10, 2);

            entity.HasOne(e => e.Request)
                  .WithMany(r => r.ClientContracts)
                  .HasForeignKey(e => e.RequestId);

            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.ClientContracts)
                  .HasForeignKey(e => e.TourId);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(10, 2);

            entity.HasOne(e => e.ClientContract)
                  .WithMany(c => c.Payments)
                  .HasForeignKey(e => e.ClientContractId);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingNum).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.ClientContract)
                  .WithMany(c => c.Bookings)
                  .HasForeignKey(e => e.ClientContractId);

            entity.HasOne(e => e.SupplierContract)
                  .WithMany(s => s.Bookings)
                  .HasForeignKey(e => e.SupplierContractId);
        });

        // SupplierContract
        modelBuilder.Entity<SupplierContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cost).HasPrecision(10, 2);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.SupplierContracts)
                  .HasForeignKey(e => e.SupplierId);

            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.SupplierContracts)
                  .HasForeignKey(e => e.TourId);
        });

        // Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}