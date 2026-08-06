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
        var dateOfArrival = new DateOnly(year, month, day);

        // Act
        var (seasonValue, seasonLabel) = SeasonCalculator.Compute(dateOfArrival);

        // Assert
        Assert.Equal(expectedSeasonValue, seasonValue);
        Assert.Equal(expectedSeasonLabel, seasonLabel);
    }

    [Theory]
    [InlineData("ete2025", 2025, 5, 1, 2025, 10, 31)]
    [InlineData("ete2026", 2026, 5, 1, 2026, 10, 31)]
    [InlineData("hiver2025-2026", 2025, 11, 1, 2026, 4, 30)]
    public void TryResolveDateRange_WithValidSeasonValue_ReturnsExpectedRange(
        string seasonValue,
        int expectedStartYear, int expectedStartMonth, int expectedStartDay,
        int expectedEndYear, int expectedEndMonth, int expectedEndDay)
    {
        // Act
        var result = SeasonCalculator.TryResolveDateRange(seasonValue, out var startDate, out var endDate);

        // Assert
        Assert.True(result);
        Assert.Equal(new DateOnly(expectedStartYear, expectedStartMonth, expectedStartDay), startDate);
        Assert.Equal(new DateOnly(expectedEndYear, expectedEndMonth, expectedEndDay), endDate);
    }

    [Theory]
    [InlineData("not-a-season")]
    [InlineData("ete20255")]
    [InlineData("eteabcd")]
    [InlineData("hiver2025-2028")]
    [InlineData("hiver2025_2026")]
    [InlineData("")]
    public void TryResolveDateRange_WithInvalidSeasonValue_ReturnsFalse(string seasonValue)
    {
        // Act
        var result = SeasonCalculator.TryResolveDateRange(seasonValue, out var startDate, out var endDate);

        // Assert
        Assert.False(result);
        Assert.Equal(default, startDate);
        Assert.Equal(default, endDate);
    }

    [Theory]
    [InlineData(2025, 5, 1)]
    [InlineData(2025, 8, 15)]
    [InlineData(2025, 10, 31)]
    [InlineData(2025, 11, 1)]
    [InlineData(2025, 12, 25)]
    [InlineData(2026, 1, 1)]
    [InlineData(2026, 4, 30)]
    public void TryResolveDateRange_RoundTripsWithCompute(int year, int month, int day)
    {
        // Arrange
        var dateOfArrival = new DateOnly(year, month, day);
        var (seasonValue, _) = SeasonCalculator.Compute(dateOfArrival);

        // Act
        var result = SeasonCalculator.TryResolveDateRange(seasonValue, out var startDate, out var endDate);

        // Assert
        Assert.True(result);
        Assert.InRange(dateOfArrival.DayNumber, startDate.DayNumber, endDate.DayNumber);
    }
}
