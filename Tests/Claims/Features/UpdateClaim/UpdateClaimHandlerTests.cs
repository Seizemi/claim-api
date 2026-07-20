using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.UpdateClaim;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.UpdateClaim;

[TestClass]
public sealed class UpdateClaimHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_WhenClaimExists_UpdatesPersistedFields()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using var writeContext = ClaimsDbContextFactory.Create(databaseName);
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-1));
        writeContext.Claims.Add(claim);
        await writeContext.SaveChangesAsync();
        writeContext.ChangeTracker.Clear();

        var request = ClaimTestDataFactory.CreateClaimRequest();
        var handler = new UpdateClaimHandler(writeContext);

        // Act
        var result = await handler.HandleAsync(claim.Id, request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);

        await using var readContext = ClaimsDbContextFactory.Create(databaseName);
        var updated = await readContext.Claims
            .Include(c => c.Booking).ThenInclude(b => b.Customer)
            .Include(c => c.Booking).ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .SingleAsync(c => c.Id == claim.Id);

        Assert.AreEqual(request.State, updated.State);
        Assert.AreEqual(request.FollowedBy, updated.FollowedBy);
        Assert.AreEqual(request.Booking.BookingNumber, updated.Booking.BookingNumber);
        Assert.AreEqual(request.Booking.Customer.Name, updated.Booking.Customer.Name);
        Assert.AreEqual(request.Booking.Supplier.Name, updated.Booking.Supplier.Name);
        Assert.AreEqual(request.ClaimDate.DateOfReceivedClaim, updated.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(request.Compensation.RefundState, updated.Compensation.RefundState);
    }

    [TestMethod]
    public async Task HandleAsync_WhenClaimDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new UpdateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(Guid.CreateVersion7(), request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        var error = Assert.ContainsSingle(result.Errors);
        Assert.AreEqual(ErrorType.Validation, error.Type);
        Assert.AreEqual(ClaimErrorCodes.ClaimCannotBeNull, error.Code);
    }

    [TestMethod]
    public async Task HandleAsync_WhenClaimExists_DoesNotChangeTotalClaimCount()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var handler = new UpdateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        await handler.HandleAsync(claim.Id, request, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, await context.Claims.CountAsync());
    }
}
