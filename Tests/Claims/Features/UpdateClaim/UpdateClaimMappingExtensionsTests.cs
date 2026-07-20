using Modules.Claims.Features.Features.UpdateClaim;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.UpdateClaim;

[TestClass]
public sealed class UpdateClaimMappingExtensionsTests
{
    [TestMethod]
    public void UpdateFrom_WithValidRequest_UpdatesTopLevelClaimFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-1));
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        claim.UpdateFrom(request);

        // Assert
        Assert.AreEqual(request.State, claim.State);
        Assert.AreEqual(request.FollowedBy, claim.FollowedBy);
        Assert.AreEqual(request.Reason, claim.Reason);
        Assert.AreEqual(request.ClaimSummary, claim.ClaimSummary);
        Assert.AreEqual(request.Solution, claim.Solution);
        Assert.AreEqual(request.PurposeOfSolution, claim.PurposeOfSolution);
        Assert.AreEqual(request.UpdateReason, claim.UpdateReason);
        Assert.AreEqual(request.CustomerSuppInfo, claim.CustomerSuppInfo);
        Assert.AreEqual(request.SupplierSuppInfo, claim.SupplierSuppInfo);
    }

    [TestMethod]
    public void UpdateFrom_WithValidRequest_UpdatesNestedBookingCustomerSupplierClaimDateCompensationFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-1));
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        claim.UpdateFrom(request);

        // Assert
        Assert.AreEqual(request.Booking.BookingNumber, claim.Booking.BookingNumber);
        Assert.AreEqual(request.Booking.SalesChannel, claim.Booking.SalesChannel);
        Assert.AreEqual(request.Booking.Language, claim.Booking.Language);
        Assert.AreEqual(request.Booking.SeasonLabel, claim.Booking.SeasonLabel);
        Assert.AreEqual(request.Booking.SeasonValue, claim.Booking.SeasonValue);
        Assert.AreEqual(request.Booking.Service, claim.Booking.Service);
        Assert.AreEqual(request.Booking.Skissim, claim.Booking.Skissim);
        Assert.AreEqual(request.Booking.SkissimType, claim.Booking.SkissimType);
        Assert.AreEqual(request.Booking.Product, claim.Booking.Product);

        Assert.AreEqual(request.Booking.Customer.Name, claim.Booking.Customer.Name);
        Assert.AreEqual(request.Booking.Customer.AkioNumber, claim.Booking.Customer.AkioNumber);

        Assert.AreEqual(request.Booking.Supplier.Name, claim.Booking.Supplier.Name);
        Assert.AreEqual(request.Booking.Supplier.SupplierAkioNumber, claim.Booking.Supplier.SupplierAkioNumber);

        Assert.AreEqual(request.ClaimDate.DateOfReceivedClaim, claim.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(request.ClaimDate.DateOfStartFollowUp, claim.ClaimDate.DateOfStartFollowUp);
        Assert.AreEqual(request.ClaimDate.DateLastUpdate, claim.ClaimDate.DateLastUpdate);
        Assert.AreEqual(request.ClaimDate.DateOfDeparture, claim.ClaimDate.DateOfDeparture);
        Assert.AreEqual(request.ClaimDate.DateEndOfFollowUp, claim.ClaimDate.DateEndOfFollowUp);
        Assert.AreEqual(request.ClaimDate.DateOfArrival, claim.ClaimDate.DateOfArrival);

        Assert.AreEqual(request.Compensation.CustomerVoucher, claim.Compensation.CustomerVoucher);
        Assert.AreEqual(request.Compensation.CustomerUsedVoucher, claim.Compensation.CustomerUsedVoucher);
        Assert.AreEqual(request.Compensation.SupplierRefund, claim.Compensation.SupplierRefund);
        Assert.AreEqual(request.Compensation.ClaimRefund, claim.Compensation.ClaimRefund);
        Assert.AreEqual(request.Compensation.RefundState, claim.Compensation.RefundState);
    }

    [TestMethod]
    public void UpdateFrom_WithValidRequest_DoesNotChangeEntityIdentitiesOrForeignKeys()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow.AddDays(-1));
        var originalClaimId = claim.Id;
        var originalBookingId = claim.Booking.Id;
        var originalCustomerId = claim.Booking.Customer.Id;
        var originalSupplierId = claim.Booking.Supplier.Id;
        var originalClaimDateId = claim.ClaimDate.Id;
        var originalCompensationId = claim.Compensation.Id;

        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        claim.UpdateFrom(request);

        // Assert
        Assert.AreEqual(originalClaimId, claim.Id);
        Assert.AreEqual(originalBookingId, claim.Booking.Id);
        Assert.AreEqual(originalCustomerId, claim.Booking.Customer.Id);
        Assert.AreEqual(originalSupplierId, claim.Booking.Supplier.Id);
        Assert.AreEqual(originalClaimDateId, claim.ClaimDate.Id);
        Assert.AreEqual(originalCompensationId, claim.Compensation.Id);
        Assert.AreEqual(claim.Booking.Id, claim.BookingId);
        Assert.AreEqual(claim.Id, claim.ClaimDate.ClaimId);
        Assert.AreEqual(claim.Id, claim.Compensation.ClaimId);
    }
}
