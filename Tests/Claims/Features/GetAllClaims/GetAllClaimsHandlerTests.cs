using Modules.Claims.Features.Features.GetAllClaims;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.GetAllClaims;

[TestClass]
public sealed class GetAllClaimsHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_WhenNoClaimsExist_ReturnsEmptyPagedResponse()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();
        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.IsEmpty(result.Value.Items);
        Assert.AreEqual(0, result.Value.TotalCount);
        Assert.AreEqual(0, result.Value.TotalPages);
    }

    [TestMethod]
    public async Task HandleAsync_WithMultipleClaims_OrdersByDateOfReceivedClaimDescending()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var oldest = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-10));
        var middle = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-5));
        var newest = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);

        context.Claims.AddRange(oldest, newest, middle);
        await context.SaveChangesAsync();

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.HasCount(3, result.Value.Items);
        CollectionAssert.AreEqual(
            new[] { newest.Id, middle.Id, oldest.Id },
            result.Value.Items.Select(c => c.Id).ToArray());
    }

    [TestMethod]
    public async Task HandleAsync_WithPageSizeSmallerThanTotalCount_ReturnsRequestedPageOnly()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claims = Enumerable.Range(0, 5)
            .Select(i => ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-i)))
            .ToList();

        context.Claims.AddRange(claims);
        await context.SaveChangesAsync();

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest(PageNumber: 2, PageSize: 2);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.HasCount(2, result.Value.Items);
        Assert.AreEqual(5, result.Value.TotalCount);
        Assert.AreEqual(3, result.Value.TotalPages);
        Assert.AreEqual(2, result.Value.PageNumber);
        Assert.AreEqual(2, result.Value.PageSize);
        // Page 2 (0-indexed skip of 2, ordered newest-first) should hold claims[2] and claims[3]
        CollectionAssert.AreEqual(
            new[] { claims[2].Id, claims[3].Id },
            result.Value.Items.Select(c => c.Id).ToArray());
    }

    [TestMethod]
    public async Task HandleAsync_WithPageNumberBeyondAvailablePages_ReturnsEmptyItemsWithCorrectTotals()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        context.Claims.AddRange(
            ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow),
            ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-1)));
        await context.SaveChangesAsync();

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest(PageNumber: 10, PageSize: 20);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.IsEmpty(result.Value.Items);
        Assert.AreEqual(2, result.Value.TotalCount);
        Assert.AreEqual(1, result.Value.TotalPages);
    }

    [TestMethod]
    public async Task HandleAsync_WhenClaimHasNestedBookingCustomerSupplier_MapsAllNestedFields()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        var response = Assert.ContainsSingle(result.Value.Items);

        Assert.AreEqual(claim.Id, response.Id);
        Assert.AreEqual(claim.Booking.BookingNumber, response.Booking.BookingNumber);
        Assert.AreEqual(claim.Booking.Customer.Name, response.Booking.Customer.Name);
        Assert.AreEqual(claim.Booking.Customer.AkioNumber, response.Booking.Customer.AkioNumber);
        Assert.AreEqual(claim.Booking.Supplier.Name, response.Booking.Supplier.Name);
        Assert.AreEqual(claim.Booking.Supplier.SupplierAkioNumber, response.Booking.Supplier.SupplierAkioNumber);
        Assert.AreEqual(claim.ClaimDate.DateOfReceivedClaim, response.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(claim.Compensation.Id, response.Compensation.Id);
    }

    [TestMethod]
    public async Task HandleAsync_Always_ReturnsEntitiesUntracked()
    {
        // Arrange
        await using var context = ClaimsDbContextFactory.Create();

        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        context.Claims.Add(claim);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new GetAllClaimsHandler(context);
        var request = new GetAllClaimsRequest();

        // Act
        await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsEmpty(context.ChangeTracker.Entries());
    }
}
