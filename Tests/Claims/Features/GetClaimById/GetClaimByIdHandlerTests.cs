using ErrorOr;
using Microsoft.Extensions.Time.Testing;
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
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimByIdHandler(context, new FakeTimeProvider());

        // Act
        var result = await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(claim.Id, result.Value.Id);
        Assert.Equal(claim.State, result.Value.State);
        Assert.Equal(claim.Booking.BookingNumber, result.Value.Booking.BookingNumber);
        Assert.Equal(claim.Booking.Customer.Name, result.Value.Booking.Customer.Name);
        Assert.Equal(claim.Booking.Supplier.Label, result.Value.Booking.Supplier.Label);
        Assert.Equal(claim.ClaimDate.DateOfReceivedClaim, result.Value.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(claim.Compensation.Id, result.Value.Compensation.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenClaimDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new GetClaimByIdHandler(context, new FakeTimeProvider());

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
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        var handler = new GetClaimByIdHandler(context, new FakeTimeProvider());

        // Act
        await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task HandleAsync_ClaimActivelyLocked_ReturnsIsLockedTrueWithHolderName()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(
            DateOnly.FromDateTime(DateTime.UtcNow),
            lockedByUserId: Guid.CreateVersion7(),
            lockedByUserName: "Alice",
            lockedAt: timeProvider.GetUtcNow());
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimByIdHandler(context, timeProvider);

        // Act
        var result = await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.IsLocked);
        Assert.Equal("Alice", result.Value.LockedByUserName);
        Assert.NotNull(result.Value.LockedAt);
    }

    [Fact]
    public async Task HandleAsync_ClaimLockExpired_ReturnsIsLockedFalse()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(
            DateOnly.FromDateTime(DateTime.UtcNow),
            lockedByUserId: Guid.CreateVersion7(),
            lockedByUserName: "Alice",
            lockedAt: timeProvider.GetUtcNow());
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromMinutes(20));

        var handler = new GetClaimByIdHandler(context, timeProvider);

        // Act
        var result = await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsLocked);
        Assert.Null(result.Value.LockedByUserName);
        Assert.Null(result.Value.LockedAt);
    }

    [Fact]
    public async Task HandleAsync_ClaimNotLocked_ReturnsIsLockedFalse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetClaimByIdHandler(context, new FakeTimeProvider());

        // Act
        var result = await handler.HandleAsync(claim.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsLocked);
        Assert.Null(result.Value.LockedByUserName);
    }
}
