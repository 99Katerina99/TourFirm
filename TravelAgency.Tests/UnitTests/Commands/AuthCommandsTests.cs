using Moq;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;
using TravelAgency.Data.Helpers;
using TravelAgency.Server.Commands;
using TravelAgency.Server.Services;
using Xunit;

namespace TravelAgency.Tests.UnitTests.Commands;

public class AuthCommandsTests
{
    [Fact]
    public void Login_ShouldReturnError_WhenInvalidCredentials()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(Array.Empty<User>());

        var sessionManager = new SessionManager();
        var parts = new[] { "LOGIN", "admin", "wrongpass" };

        // Act
        var result = AuthCommands.Login(parts, mockRepo.Object, sessionManager);

        // Assert
        Assert.StartsWith("ERROR", result);
    }

    [Fact]
    public void Login_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<User>>();
        var passwordHash = PasswordHelper.HashPassword("admin123");

        mockRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new[] { new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash,
                Role = "admin"
            }});

        var sessionManager = new SessionManager();
        var parts = new[] { "LOGIN", "admin", "admin123" };

        // Act
        var result = AuthCommands.Login(parts, mockRepo.Object, sessionManager);

        // Assert
        Assert.StartsWith("OK|", result);
        Assert.Contains("\"Token\"", result);
        Assert.Contains("\"admin\"", result);
    }

    [Fact]
    public void Login_ShouldReturnError_WhenMissingParameters()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<User>>();
        var sessionManager = new SessionManager();
        var parts = new[] { "LOGIN", "admin" }; // Нет пароля

        // Act
        var result = AuthCommands.Login(parts, mockRepo.Object, sessionManager);

        // Assert
        Assert.Equal("ERROR|Использование: LOGIN|username|password", result);
    }
}