using Modules.Claims.Domain;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.GetClaimsBySeason;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimsBySeason;

public sealed class GetClaimsBySeasonHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoClaimsMatchSeason_ReturnsEmptyList()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow), ClaimState.AwaitingClient);
        claim.ClaimDate.DateOfArrival = new DateOnly(2025, 8, 15);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsBySeasonHandler(context);
        var request = new GetClaimsBySeasonRequest("hiver2025-2026");

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WithClaimsInDifferentSeasons_ReturnsOnlyMatchingSeason()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var matching = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        matching.ClaimDate.DateOfArrival = new DateOnly(2025, 8, 15);
        var otherSeason = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        otherSeason.ClaimDate.DateOfArrival = new DateOnly(2025, 12, 25);

        context.Claims.AddRange(matching, otherSeason);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsBySeasonHandler(context);
        var request = new GetClaimsBySeasonRequest("ete2025");

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        var response = Assert.Single(result.Value);
        Assert.Equal(matching.Id, response.Id);
    }

    [Theory]
    [InlineData(2025, 5, 1)]
    [InlineData(2025, 10, 31)]
    public async Task HandleAsync_WithDateOnSeasonBoundary_IncludesClaim(int year, int month, int day)
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        claim.ClaimDate.DateOfArrival = new DateOnly(year, month, day);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsBySeasonHandler(context);
        var request = new GetClaimsBySeasonRequest("ete2025");

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        var response = Assert.Single(result.Value);
        Assert.Equal(claim.Id, response.Id);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleMatchingClaims_OrdersByDateOfReceivedClaimDescending()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var oldest = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10));
        oldest.ClaimDate.DateOfArrival = new DateOnly(2025, 6, 1);
        var middle = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5));
        middle.ClaimDate.DateOfArrival = new DateOnly(2025, 7, 1);
        var newest = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        newest.ClaimDate.DateOfArrival = new DateOnly(2025, 8, 1);

        context.Claims.AddRange(oldest, newest, middle);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsBySeasonHandler(context);
        var request = new GetClaimsBySeasonRequest("ete2025");

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(
            new[] { newest.Id, middle.Id, oldest.Id },
            result.Value.Select(c => c.Id).ToArray());
    }

    [Fact]
    public async Task HandleAsync_WhenMatchingClaimHasNestedBookingCustomerSupplier_MapsFullClaimResponse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        claim.ClaimDate.DateOfArrival = new DateOnly(2025, 8, 15);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsBySeasonHandler(context);
        var request = new GetClaimsBySeasonRequest("ete2025");

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        var response = Assert.Single(result.Value);
        var expectedSeason = SeasonCalculator.Compute(claim.ClaimDate.DateOfArrival.Value);

        Assert.Equal(claim.Id, response.Id);
        Assert.Equal(claim.State, response.State);
        Assert.Equal(claim.Booking.Customer.Name, response.Booking.Customer.Name);
        Assert.Equal(claim.Booking.SalesChannel.Language, response.Booking.SalesChannel.Language);
        Assert.Equal(claim.Booking.BookingNumber, response.Booking.BookingNumber);
        Assert.Equal(claim.Booking.Supplier.Label, response.Booking.Supplier.Label);
        Assert.Equal(claim.ClaimDate.DateOfReceivedClaim, response.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(claim.Compensation.Id, response.Compensation.Id);
        Assert.Equal(expectedSeason.SeasonLabel, response.Booking.SeasonLabel);
        Assert.Equal(expectedSeason.SeasonValue, response.Booking.SeasonValue);
    }

    [Fact]
    public async Task HandleAsync_Always_ReturnsEntitiesUntracked()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        claim.ClaimDate.DateOfArrival = new DateOnly(2025, 8, 15);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        var handler = new GetClaimsBySeasonHandler(context);
        var request = new GetClaimsBySeasonRequest("ete2025");

        // Act
        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
