using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.CreateClaim;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.CreateClaim;

[TestClass]
public sealed class CreateClaimHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_WithValidRequest_ReturnsNonEmptyClaimId()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreNotEqual(Guid.Empty, result.Value);
    }

    [TestMethod]
    public async Task HandleAsync_WithValidRequest_PersistsClaimWithMappedFields()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using var writeContext = ClaimsDbContextFactory.Create(databaseName);
        var handler = new CreateClaimHandler(writeContext);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);

        await using var readContext = ClaimsDbContextFactory.Create(databaseName);
        var persisted = await readContext.Claims
            .Include(c => c.Booking).ThenInclude(b => b.Customer)
            .Include(c => c.Booking).ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .SingleAsync(c => c.Id == result.Value);

        Assert.AreEqual(request.State, persisted.State);
        Assert.AreEqual(request.FollowedBy, persisted.FollowedBy);
        Assert.AreEqual(request.Reason, persisted.Reason);
        Assert.AreEqual(request.Booking.BookingNumber, persisted.Booking.BookingNumber);
        Assert.AreEqual(request.Booking.Customer.Name, persisted.Booking.Customer.Name);
        Assert.AreEqual(request.Booking.Supplier.Name, persisted.Booking.Supplier.Name);
        Assert.AreEqual(request.ClaimDate.DateOfReceivedClaim, persisted.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(request.Compensation.RefundState, persisted.Compensation.RefundState);
    }

    [TestMethod]
    public async Task HandleAsync_WithValidRequest_AssignsDistinctNonEmptyIdsToNestedEntities()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        var claim = await context.Claims
            .Include(c => c.Booking).ThenInclude(b => b.Customer)
            .Include(c => c.Booking).ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .SingleAsync(c => c.Id == result.Value);

        var ids = new[]
        {
            claim.Id,
            claim.Booking.Id,
            claim.Booking.Customer.Id,
            claim.Booking.Supplier.Id,
            claim.ClaimDate.Id,
            claim.Compensation.Id
        };

        Assert.HasCount(ids.Length, ids.Distinct());
        Assert.DoesNotContain(Guid.Empty, ids);
    }

    [TestMethod]
    public async Task HandleAsync_CalledTwice_CreatesTwoDistinctClaims()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);

        // Act
        var firstResult = await handler.HandleAsync(ClaimTestDataFactory.CreateClaimRequest(), CancellationToken.None);
        var secondResult = await handler.HandleAsync(ClaimTestDataFactory.CreateClaimRequest(), CancellationToken.None);

        // Assert
        Assert.AreNotEqual(firstResult.Value, secondResult.Value);
        Assert.AreEqual(2, await context.Claims.CountAsync());
    }
}
