using ErrorOr;
using Modules.Claims.Features.Features.GetClaimById;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimById;

public sealed class GetClaimByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenClaimExists_ReturnsMappedClaimResponse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimByIdHandler(context);

        // Act
        var result = await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(claim.Id, result.Value.Id);
        Assert.Equal(claim.State, result.Value.State);
        Assert.Equal(claim.Booking.BookingNumber, result.Value.Booking.BookingNumber);
        Assert.Equal(claim.Booking.Customer.Name, result.Value.Booking.Customer.Name);
        Assert.Equal(claim.Booking.Supplier.Name, result.Value.Booking.Supplier.Name);
        Assert.Equal(claim.ClaimDate.DateOfReceivedClaim, result.Value.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(claim.Compensation.Id, result.Value.Compensation.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenClaimDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new GetClaimByIdHandler(context);

        // Act
        var result = await handler.HandleAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(ClaimErrorCodes.ClaimCannotBeNull, error.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenClaimExists_ReturnsEntityUntracked()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        var handler = new GetClaimByIdHandler(context);

        // Act
        await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
