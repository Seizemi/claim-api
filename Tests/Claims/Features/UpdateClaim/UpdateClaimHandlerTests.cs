using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.UpdateClaim;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.UpdateClaim;

public sealed class UpdateClaimHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenClaimExists_UpdatesPersistedFields()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using var writeContext = ClaimsDbContextFactory.Create(databaseName);
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        writeContext.Claims.Add(claim);
        await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        writeContext.ChangeTracker.Clear();

        var request = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(writeContext, request, TestContext.Current.CancellationToken);
        var handler = new UpdateClaimHandler(writeContext, new FakeTimeProvider());

        // Act
        var result = await handler.HandleAsync(claim.Id, request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);

        await using var readContext = ClaimsDbContextFactory.Create(databaseName);
        var updated = await readContext.Claims
            .Include(c => c.Booking).ThenInclude(b => b.Customer)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .SingleAsync(c => c.Id == claim.Id, TestContext.Current.CancellationToken);

        Assert.Equal(request.State, updated.State);
        Assert.Equal(request.FollowedById, updated.FollowedById);
        Assert.Equal(request.Booking.BookingNumber, updated.Booking.BookingNumber);
        Assert.Equal(request.Booking.Customer.Name, updated.Booking.Customer.Name);
        Assert.Equal(request.Booking.Supplier.Id, updated.Booking.SupplierId);
        Assert.Equal(request.ClaimDate.DateOfReceivedClaim, updated.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(request.Compensation.RefundStateId, updated.Compensation.RefundStateId);
    }

    [Fact]
    public async Task HandleAsync_WhenClaimDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new UpdateClaimHandler(context, new FakeTimeProvider());
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = await handler.HandleAsync(Guid.CreateVersion7(), request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(ClaimErrorCodes.ClaimCannotBeNull, error.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenClaimExists_DoesNotChangeTotalClaimCount()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow));
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateClaimHandler(context, new FakeTimeProvider());
        var request = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);

        // Act
        await handler.HandleAsync(claim.Id, request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, await context.Claims.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_LockedByAnotherActiveUser_ReturnsConflictError()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        await using var context = ClaimsDbContextFactory.Create();
        var lockHolderId = Guid.CreateVersion7();
        var claim = ClaimTestDataFactory.CreateClaim(
            DateOnly.FromDateTime(DateTime.UtcNow),
            lockedByUserId: lockHolderId,
            lockedByUserName: "Alice",
            lockedAt: timeProvider.GetUtcNow());
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateClaimHandler(context, timeProvider);
        var request = ClaimTestDataFactory.CreateClaimRequest() with { EditingUserId = Guid.CreateVersion7() };

        // Act
        var result = await handler.HandleAsync(claim.Id, request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal(ClaimErrorCodes.ClaimLockedByAnotherUser, error.Code);
    }

    [Fact]
    public async Task HandleAsync_LockedByRequestingUser_Succeeds()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        await using var context = ClaimsDbContextFactory.Create();
        var lockHolderId = Guid.CreateVersion7();
        var claim = ClaimTestDataFactory.CreateClaim(
            DateOnly.FromDateTime(DateTime.UtcNow),
            lockedByUserId: lockHolderId,
            lockedByUserName: "Alice",
            lockedAt: timeProvider.GetUtcNow());
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateClaimHandler(context, timeProvider);
        var request = ClaimTestDataFactory.CreateClaimRequest() with { EditingUserId = lockHolderId };
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);

        // Act
        var result = await handler.HandleAsync(claim.Id, request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_LockExpired_Succeeds()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        await using var context = ClaimsDbContextFactory.Create();
        var lockHolderId = Guid.CreateVersion7();
        var claim = ClaimTestDataFactory.CreateClaim(
            DateOnly.FromDateTime(DateTime.UtcNow),
            lockedByUserId: lockHolderId,
            lockedByUserName: "Alice",
            lockedAt: timeProvider.GetUtcNow());
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromMinutes(20));

        var handler = new UpdateClaimHandler(context, timeProvider);
        var request = ClaimTestDataFactory.CreateClaimRequest() with { EditingUserId = Guid.CreateVersion7() };
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);

        // Act
        var result = await handler.HandleAsync(claim.Id, request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
    }
}
