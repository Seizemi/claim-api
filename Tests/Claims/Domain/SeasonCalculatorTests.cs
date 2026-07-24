using Modules.Claims.Domain;
using Xunit;

namespace Modules.Claims.Features.Tests.Domain;

public sealed class SeasonCalculatorTests
{
    [Theory]
    [InlineData(2025, 5, 1, "ete2025", "Été 2025")]
    [InlineData(2025, 8, 15, "ete2025", "Été 2025")]
    [InlineData(2025, 10, 31, "ete2025", "Été 2025")]
    [InlineData(2025, 11, 1, "hiver2025-2026", "Hiver 2025-2026")]
    [InlineData(2025, 12, 25, "hiver2025-2026", "Hiver 2025-2026")]
    [InlineData(2026, 1, 1, "hiver2025-2026", "Hiver 2025-2026")]
    [InlineData(2026, 4, 30, "hiver2025-2026", "Hiver 2025-2026")]
    [InlineData(2026, 5, 1, "ete2026", "Été 2026")]
    public void Compute_WithDateOfArrival_ReturnsExpectedSeason(
        int year, int month, int day, string expectedSeasonValue, string expectedSeasonLabel)
    {
        // Arrange
        var dateOfArrival = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);

        // Act
        var (seasonValue, seasonLabel) = SeasonCalculator.Compute(dateOfArrival);

        // Assert
        Assert.Equal(expectedSeasonValue, seasonValue);
        Assert.Equal(expectedSeasonLabel, seasonLabel);
    }
}
