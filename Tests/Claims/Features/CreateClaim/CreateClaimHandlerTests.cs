using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.CreateClaim;
using Modules.Claims.Features.Features.Shared.Errors;
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
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);

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
        await ClaimTestDataFactory.SeedLookupsAsync(writeContext, request, TestContext.Current.CancellationToken);

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
        Assert.Equal(request.FollowedById, persisted.FollowedById);
        Assert.Equal(request.ReasonId, persisted.ReasonId);
        Assert.Equal(request.Booking.BookingNumber, persisted.Booking.BookingNumber);
        Assert.Equal(request.Booking.Customer.Name, persisted.Booking.Customer.Name);
        Assert.Equal(request.Booking.Supplier.Label, persisted.Booking.Supplier.Label);
        Assert.Equal(request.Booking.Supplier.Value, persisted.Booking.Supplier.Value);
        Assert.Equal(request.ClaimDate.DateOfReceivedClaim, persisted.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(request.Compensation.RefundStateId, persisted.Compensation.RefundStateId);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_AssignsDistinctNonEmptyIdsToNestedEntities()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);

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
        var firstRequest = ClaimTestDataFactory.CreateClaimRequest();
        var secondRequest = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(context, firstRequest, TestContext.Current.CancellationToken);
        await ClaimTestDataFactory.SeedLookupsAsync(context, secondRequest, TestContext.Current.CancellationToken);

        // Act
        var firstResult = await handler.HandleAsync(firstRequest, TestContext.Current.CancellationToken);
        var secondResult = await handler.HandleAsync(secondRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(firstResult.Value, secondResult.Value);
        Assert.Equal(2, await context.Claims.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentReasonId_ReturnsValidationErrorAndPersistsNothing()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);
        request = request with { ReasonId = Guid.NewGuid() };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == ClaimErrorCodes.ClaimReasonIdDoesNotExist);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal(0, await context.Claims.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentFollowedById_ReturnsValidationErrorAndPersistsNothing()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new CreateClaimHandler(context);
        var request = ClaimTestDataFactory.CreateClaimRequest();
        await ClaimTestDataFactory.SeedLookupsAsync(context, request, TestContext.Current.CancellationToken);
        request = request with { FollowedById = Guid.NewGuid() };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == ClaimErrorCodes.ClaimFollowedByIdDoesNotExist);
        Assert.Equal(0, await context.Claims.CountAsync(TestContext.Current.CancellationToken));
    }
}
