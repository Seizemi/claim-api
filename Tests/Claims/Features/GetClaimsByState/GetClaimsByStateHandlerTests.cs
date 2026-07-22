using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.GetClaimsByState;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimsByState;

public sealed class GetClaimsByStateHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoClaimsMatchState_ReturnsEmptyPagedResponse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        context.Claims.Add(ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow, ClaimState.AwaitingClient));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsByStateHandler(context);
        var request = new GetClaimsByStateRequest(ClaimState.AwaitingSupplier);

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_WithClaimsInMultipleStates_ReturnsOnlyMatchingState()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var matching = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow, ClaimState.AwaitingSupplier);
        var otherState = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow, ClaimState.AwaitingClient);

        context.Claims.AddRange(matching, otherState);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsByStateHandler(context);
        var request = new GetClaimsByStateRequest(ClaimState.AwaitingSupplier);

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        var response = Assert.Single(result.Value.Items);
        Assert.Equal(matching.Id, response.Id);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleMatchingClaims_OrdersByDateOfReceivedClaimDescending()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var oldest = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-10), ClaimState.Terminate);
        var middle = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-5), ClaimState.Terminate);
        var newest = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow, ClaimState.Terminate);

        context.Claims.AddRange(oldest, newest, middle);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsByStateHandler(context);
        var request = new GetClaimsByStateRequest(ClaimState.Terminate);

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(
            new[] { newest.Id, middle.Id, oldest.Id },
            result.Value.Items.Select(c => c.Id).ToArray());
    }

    [Fact]
    public async Task HandleAsync_WithPageSizeSmallerThanTotalCount_ReturnsRequestedPageOnly()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claims = Enumerable.Range(0, 5)
            .Select(i => ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-i), ClaimState.ClosedWithoutResponse))
            .ToList();

        context.Claims.AddRange(claims);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimsByStateHandler(context);
        var request = new GetClaimsByStateRequest(ClaimState.ClosedWithoutResponse, PageNumber: 2, PageSize: 2);

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Items.Count());
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(
            new[] { claims[2].Id, claims[3].Id },
            result.Value.Items.Select(c => c.Id).ToArray());
    }

    [Fact]
    public async Task HandleAsync_Always_ReturnsEntitiesUntracked()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow, ClaimState.AwaitingSupplier);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        var handler = new GetClaimsByStateHandler(context);
        var request = new GetClaimsByStateRequest(ClaimState.AwaitingSupplier);

        // Act
        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
