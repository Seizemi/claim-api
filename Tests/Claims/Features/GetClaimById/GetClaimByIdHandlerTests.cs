using ErrorOr;
using Modules.Claims.Features.Features.GetClaimById;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.GetClaimById;

[TestClass]
public sealed class GetClaimByIdHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_WhenClaimExists_ReturnsMappedClaimResponse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var handler = new GetClaimByIdHandler(context);

        // Act
        var result = await handler.HandleAsync(claim.Id, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(claim.Id, result.Value.Id);
        Assert.AreEqual(claim.State, result.Value.State);
        Assert.AreEqual(claim.Booking.BookingNumber, result.Value.Booking.BookingNumber);
        Assert.AreEqual(claim.Booking.Customer.Name, result.Value.Booking.Customer.Name);
        Assert.AreEqual(claim.Booking.Supplier.Name, result.Value.Booking.Supplier.Name);
        Assert.AreEqual(claim.ClaimDate.DateOfReceivedClaim, result.Value.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(claim.Compensation.Id, result.Value.Compensation.Id);
    }

    [TestMethod]
    public async Task HandleAsync_WhenClaimDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new GetClaimByIdHandler(context);

        // Act
        var result = await handler.HandleAsync(Guid.CreateVersion7(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        var error = Assert.ContainsSingle(result.Errors);
        Assert.AreEqual(ErrorType.Validation, error.Type);
        Assert.AreEqual(ClaimErrorCodes.ClaimCannotBeNull, error.Code);
    }

    [TestMethod]
    public async Task HandleAsync_WhenClaimExists_ReturnsEntityUntracked()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new GetClaimByIdHandler(context);

        // Act
        await handler.HandleAsync(claim.Id, CancellationToken.None);

        // Assert
        Assert.IsEmpty(context.ChangeTracker.Entries());
    }
}
