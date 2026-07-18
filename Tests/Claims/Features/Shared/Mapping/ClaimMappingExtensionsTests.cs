using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.Shared.Mapping;

[TestClass]
public sealed class ClaimMappingExtensionsTests
{
    [TestMethod]
    public void MapToResponse_WithValidClaim_MapsTopLevelFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);

        // Act
        var response = claim.MapToResponse();

        // Assert
        Assert.AreEqual(claim.Id, response.Id);
        Assert.AreEqual(claim.State, response.State);
        Assert.AreEqual(claim.FollowedBy, response.FollowedBy);
        Assert.AreEqual(claim.Reason, response.Reason);
        Assert.AreEqual(claim.ClaimSummary, response.ClaimSummary);
        Assert.AreEqual(claim.Solution, response.Solution);
        Assert.AreEqual(claim.PurposeOfSolution, response.PurposeOfSolution);
        Assert.AreEqual(claim.UpdateReason, response.UpdateReason);
        Assert.AreEqual(claim.CustomerSuppInfo, response.CustomerSuppInfo);
        Assert.AreEqual(claim.SupplierSuppInfo, response.SupplierSuppInfo);
    }

    [TestMethod]
    public void MapToResponse_WithValidClaim_MapsNestedBookingCustomerSupplierClaimDateCompensationFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);

        // Act
        var response = claim.MapToResponse();

        // Assert
        Assert.AreEqual(claim.Booking.Id, response.Booking.Id);
        Assert.AreEqual(claim.Booking.BookingNumber, response.Booking.BookingNumber);
        Assert.AreEqual(claim.Booking.SalesChannel, response.Booking.SalesChannel);
        Assert.AreEqual(claim.Booking.Customer.Id, response.Booking.Customer.Id);
        Assert.AreEqual(claim.Booking.Customer.Name, response.Booking.Customer.Name);
        Assert.AreEqual(claim.Booking.Customer.AkioNumber, response.Booking.Customer.AkioNumber);
        Assert.AreEqual(claim.Booking.Supplier.Id, response.Booking.Supplier.Id);
        Assert.AreEqual(claim.Booking.Supplier.Name, response.Booking.Supplier.Name);
        Assert.AreEqual(claim.Booking.Supplier.SupplierAkioNumber, response.Booking.Supplier.SupplierAkioNumber);

        Assert.AreEqual(claim.ClaimDate.Id, response.ClaimDate.Id);
        Assert.AreEqual(claim.ClaimDate.DateOfReceivedClaim, response.ClaimDate.DateOfReceivedClaim);

        Assert.AreEqual(claim.Compensation.Id, response.Compensation.Id);
        Assert.AreEqual(claim.Compensation.RefundState, response.Compensation.RefundState);
    }
}
