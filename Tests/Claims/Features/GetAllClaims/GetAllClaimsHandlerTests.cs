using Modules.Claims.Features.Features.GetAllClaims;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetAllClaims;

public sealed class GetAllClaimsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoClaimsExist_ReturnsEmptyPagedResponse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleClaims_OrdersByDateOfReceivedClaimDescending()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var oldest = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-10));
        var middle = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-5));
        var newest = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);

        context.Claims.AddRange(oldest, newest, middle);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Items.Count());
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
            .Select(i => ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-i)))
            .ToList();

        context.Claims.AddRange(claims);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest(PageNumber: 2, PageSize: 2);

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Items.Count());
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(2, result.Value.PageSize);
        // Page 2 (0-indexed skip of 2, ordered newest-first) should hold claims[2] and claims[3]
        Assert.Equal(
            new[] { claims[2].Id, claims[3].Id },
            result.Value.Items.Select(c => c.Id).ToArray());
    }

    [Fact]
    public async Task HandleAsync_WithPageNumberBeyondAvailablePages_ReturnsEmptyItemsWithCorrectTotals()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        context.Claims.AddRange(
            ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow),
            ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-1)));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest(PageNumber: 10, PageSize: 20);

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_WhenClaimHasNestedBookingCustomerSupplier_MapsAllNestedFields()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        var response = Assert.Single(result.Value.Items);

        Assert.Equal(claim.Id, response.Id);
        Assert.Equal(claim.Booking.BookingNumber, response.Booking.BookingNumber);
        Assert.Equal(claim.Booking.Customer.Name, response.Booking.Customer.Name);
        Assert.Equal(claim.Booking.Customer.AkioNumber, response.Booking.Customer.AkioNumber);
        Assert.Equal(claim.Booking.Supplier.Name, response.Booking.Supplier.Name);
        Assert.Equal(claim.Booking.Supplier.SupplierAkioNumber, response.Booking.Supplier.SupplierAkioNumber);
        Assert.Equal(claim.ClaimDate.DateOfReceivedClaim, response.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(claim.Compensation.Id, response.Compensation.Id);
    }

    [Fact]
    public async Task HandleAsync_Always_ReturnsEntitiesUntracked()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
