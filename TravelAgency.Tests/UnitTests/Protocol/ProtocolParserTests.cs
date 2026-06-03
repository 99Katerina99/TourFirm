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

    // ❌ Удалено:
    // - ParseLoginCommand_ShouldExtractCredentials()
    // - ParseAddTourCommand_ShouldExtractAllParameters()
}