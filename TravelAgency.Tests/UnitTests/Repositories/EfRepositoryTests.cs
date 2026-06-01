using TravelAgency.Core.Entities;
using TravelAgency.Tests.Fixtures;
using Xunit;

namespace TravelAgency.Tests.UnitTests.Repositories;

public class EfRepositoryTests : IClassFixture<RepositoryTestsFixture>
{
    private readonly RepositoryTestsFixture _fixture;

    public EfRepositoryTests(RepositoryTestsFixture fixture)
    {
        _fixture = fixture;
        CleanupDatabase();
    }



    // 🔥 Метод очистки таблиц
    private void CleanupDatabase()
    {
        _fixture.DbContext.Bookings.RemoveRange(_fixture.DbContext.Bookings);
        _fixture.DbContext.Tours.RemoveRange(_fixture.DbContext.Tours);
        _fixture.DbContext.Clients.RemoveRange(_fixture.DbContext.Clients);
        _fixture.DbContext.Users.RemoveRange(_fixture.DbContext.Users);
        _fixture.DbContext.SaveChanges();
    }



    [Fact]
    public async Task AddAsync_ShouldInsertEntity()
    {
        // Arrange
        var repo = _fixture.EfTourRepo;
        var tour = new Tour
        {
            Name = "Test Tour",
            Country = "Testland",
            Price = 1000,
            DurationDays = 7,
            StartDate = DateTime.UtcNow.AddDays(30),
            AvailableSeats = 10
        };

        // Act
        var result = await repo.AddAsync(tour);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Tour", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        // Arrange
        var repo = _fixture.EfTourRepo;
        var tour = await repo.AddAsync(new Tour
        {
            Name = "Get Tour",
            Country = "Test",
            Price = 500,
            DurationDays = 3,
            StartDate = DateTime.UtcNow.AddDays(10),
            AvailableSeats = 5
        });

        // Act
        var result = await repo.GetByIdAsync(tour.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tour.Id, result.Id);
        Assert.Equal("Get Tour", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var repo = _fixture.EfTourRepo;
        await repo.AddAsync(new Tour { Name = "Tour 1", Country = "A", Price = 100, DurationDays = 1, StartDate = DateTime.UtcNow, AvailableSeats = 1 });
        await repo.AddAsync(new Tour { Name = "Tour 2", Country = "B", Price = 200, DurationDays = 2, StartDate = DateTime.UtcNow, AvailableSeats = 2 });

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyEntity()
    {
        // Arrange
        var repo = _fixture.EfTourRepo;
        var tour = await repo.AddAsync(new Tour
        {
            Name = "Old Name",
            Country = "Test",
            Price = 100,
            DurationDays = 1,
            StartDate = DateTime.UtcNow,
            AvailableSeats = 1
        });
        tour.Name = "New Name";
        tour.Price = 999;

        // Act
        await repo.UpdateAsync(tour);
        var updated = await repo.GetByIdAsync(tour.Id);

        // Assert
        Assert.Equal("New Name", updated.Name);
        Assert.Equal(999, updated.Price);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        // Arrange
        var repo = _fixture.EfTourRepo;
        var tour = await repo.AddAsync(new Tour
        {
            Name = "Delete Me",
            Country = "Test",
            Price = 100,
            DurationDays = 1,
            StartDate = DateTime.UtcNow,
            AvailableSeats = 1
        });

        // Act
        await repo.DeleteAsync(tour.Id);
        var result = await repo.GetByIdAsync(tour.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindAsync_ShouldFilterEntities()
    {
        // Arrange
        var repo = _fixture.EfTourRepo;
        await repo.AddAsync(new Tour { Name = "France Tour", Country = "France", Price = 1000, DurationDays = 7, StartDate = DateTime.UtcNow, AvailableSeats = 10 });
        await repo.AddAsync(new Tour { Name = "Spain Tour", Country = "Spain", Price = 800, DurationDays = 5, StartDate = DateTime.UtcNow, AvailableSeats = 8 });

        // Act
        var result = await repo.FindAsync(t => t.Country == "France");

        // Assert
        Assert.Single(result);
        Assert.All(result, t => Assert.Equal("France", t.Country));
    }
}