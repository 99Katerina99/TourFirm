using Xunit;

namespace TravelAgency.Tests.UnitTests.Protocol;

public class ProtocolParserTests
{
    [Theory]
    [InlineData("TEST", "TEST")]
    [InlineData("LOGIN|admin|pass", "LOGIN")]
    [InlineData("GET_TOURS_ORM|token123", "GET_TOURS_ORM")]
    [InlineData("ADD_TOUR_SQL|tok|Name|Desc|FR|Paris|7|1000|2026-09-01|10", "ADD_TOUR_SQL")]
    public void SplitCommand_ShouldExtractCommandName(string input, string expected)
    {
        // Act
        var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToUpper();

        // Assert
        Assert.Equal(expected, command);
    }

    [Fact]
    public void ParseLoginCommand_ShouldExtractCredentials()
    {
        // Arrange
        var input = "LOGIN|myuser|mypassword";

        // Act
        var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        Assert.Equal(3, parts.Length);
        Assert.Equal("LOGIN", parts[0]);
        Assert.Equal("myuser", parts[1]);
        Assert.Equal("mypassword", parts[2]);
    }

    [Fact]
    public void ParseAddTourCommand_ShouldExtractAllParameters()
    {
        // Arrange
        var input = "ADD_TOUR_SQL|token|Paris|Romance|France|Paris|7|1200.50|2026-09-01|10";

        // Act
        var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        Assert.Equal(10, parts.Length);
        Assert.Equal("ADD_TOUR_SQL", parts[0]);
        Assert.Equal("token", parts[1]);
        Assert.Equal("Paris", parts[2]);
        Assert.Equal("1200.50", parts[7]);
    }
}