using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.CreateClaim;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.CreateClaim;

public sealed class CreateClaimHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsNonEmptyClaimId()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_PersistsClaimWithMappedFields()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using var writeContext = ClaimsDbContextFactory.Create(databaseName);
        var handler = new CreateClaimHandler(writeContext);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);

        await using var readContext = ClaimsDbContextFactory.Create(databaseName);
        var persisted = await readContext.Claims
            .Include(c => c.Booking).ThenInclude(b => b.Customer)
            .Include(c => c.Booking).ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .SingleAsync(c => c.Id == result.Value, TestContext.Current.CancellationToken);

        Assert.Equal(request.State, persisted.State);
        Assert.Equal(request.FollowedBy, persisted.FollowedBy);
        Assert.Equal(request.Reason, persisted.Reason);
        Assert.Equal(request.Booking.BookingNumber, persisted.Booking.BookingNumber);
        Assert.Equal(request.Booking.Customer.Name, persisted.Booking.Customer.Name);
        Assert.Equal(request.Booking.Supplier.Name, persisted.Booking.Supplier.Name);
        Assert.Equal(request.ClaimDate.DateOfReceivedClaim, persisted.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(request.Compensation.RefundState, persisted.Compensation.RefundState);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_AssignsDistinctNonEmptyIdsToNestedEntities()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var claim = await context.Claims
            .Include(c => c.Booking).ThenInclude(b => b.Customer)
            .Include(c => c.Booking).ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .SingleAsync(c => c.Id == result.Value, TestContext.Current.CancellationToken);

        var ids = new[]
        {
            claim.Id,
            claim.Booking.Id,
            claim.Booking.Customer.Id,
            claim.Booking.Supplier.Id,
            claim.ClaimDate.Id,
            claim.Compensation.Id
        };

        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, ids);
    }

    [Fact]
    public async Task HandleAsync_CalledTwice_CreatesTwoDistinctClaims()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);

        // Act
        var firstResult = await handler.HandleAsync(ClaimTestDataFactory.CreateClaimRequest(), TestContext.Current.CancellationToken);
        var secondResult = await handler.HandleAsync(ClaimTestDataFactory.CreateClaimRequest(), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(firstResult.Value, secondResult.Value);
        Assert.Equal(2, await context.Claims.CountAsync(TestContext.Current.CancellationToken));
    }
}
