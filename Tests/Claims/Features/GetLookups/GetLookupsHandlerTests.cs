using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Features.GetLookups;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetLookups;

public sealed class GetLookupsHandlerTests
{
    private readonly FakeClock _clock = new();

    [Fact]
    public async Task HandleAsync_WhenNoLookupsExist_ReturnsEmptyLists()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        using var cache = CreateCache();
        var handler = new GetLookupsHandler(context, cache);

        // Act
        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Reasons);
        Assert.Empty(result.Solutions);
        Assert.Empty(result.FollowedBies);
        Assert.Empty(result.RefundStates);
        Assert.Empty(result.CompensationReasons);
        Assert.Empty(result.SalesChannels);
        Assert.Empty(result.Services);
        Assert.Empty(result.Suppliers);
        Assert.Empty(result.SkissimTypes);
    }

    [Fact]
    public async Task HandleAsync_WithLookups_ReturnsEveryList()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        using var cache = CreateCache();
        var request = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);
        var handler = new GetLookupsHandler(context, cache);

        // Act
        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(request.ReasonId, Assert.Single(result.Reasons).Id);
        Assert.Equal(request.SolutionId, Assert.Single(result.Solutions).Id);
        Assert.Equal(request.FollowedById, Assert.Single(result.FollowedBies).Id);
        Assert.Equal(request.Compensation.RefundStateId, Assert.Single(result.RefundStates).Id);
        Assert.Equal(request.Compensation.CompensationReasonId, Assert.Single(result.CompensationReasons).Id);
        Assert.Equal(request.Booking.SalesChannelId, Assert.Single(result.SalesChannels).Id);
        Assert.Equal(request.Booking.SkissimTypeId, Assert.Single(result.SkissimTypes).Id);

        var service = Assert.Single(result.Services);
        var supplier = Assert.Single(result.Suppliers);
        Assert.Equal(request.Booking.Supplier.Id, supplier.Id);
        Assert.Equal(service.Id, supplier.Service.Id);
    }

    [Fact]
    public async Task HandleAsync_JustBeforeTheCacheExpires_ReturnsTheCachedLookups()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        using var cache = CreateCache();
        var handler = new GetLookupsHandler(context, cache);
        await handler.HandleAsync(TestContext.Current.CancellationToken);

        await AddReasonAsync(context);
        _clock.UtcNow += TimeSpan.FromHours(8) - TimeSpan.FromSeconds(1);

        // Act
        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public async Task HandleAsync_JustAfterTheCacheExpires_ReadsTheLookupsAgain()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        using var cache = CreateCache();
        var handler = new GetLookupsHandler(context, cache);
        await handler.HandleAsync(TestContext.Current.CancellationToken);

        await AddReasonAsync(context);
        _clock.UtcNow += TimeSpan.FromHours(8) + TimeSpan.FromSeconds(1);

        // Act
        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Reasons);
    }

    private MemoryCache CreateCache() => new(new MemoryCacheOptions { Clock = _clock });

    private static async Task AddReasonAsync(Modules.Claims.Infrastructure.Database.ClaimsDbContext context)
    {
        context.Reasons.Add(new Reason { Id = Guid.CreateVersion7(), Label = "Reason", Value = "Reason" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private sealed class FakeClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
