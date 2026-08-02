using Modules.Claims.Features.Features.UpdateClaim;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.UpdateClaim;

public sealed class UpdateClaimMappingExtensionsTests
{
    [Fact]
    public void UpdateFrom_WithValidRequest_UpdatesTopLevelClaimFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        claim.UpdateFrom(request);

        // Assert
        Assert.Equal(request.State, claim.State);
        Assert.Equal(request.FollowedById, claim.FollowedById);
        Assert.Equal(request.ReasonId, claim.ReasonId);
        Assert.Equal(request.ClaimSummary, claim.ClaimSummary);
        Assert.Equal(request.SolutionId, claim.SolutionId);
        Assert.Equal(request.PurposeOfSolution, claim.PurposeOfSolution);
        Assert.Equal(request.UpdateReason, claim.UpdateReason);
        Assert.Equal(request.CustomerSuppInfo, claim.CustomerSuppInfo);
        Assert.Equal(request.SupplierSuppInfo, claim.SupplierSuppInfo);
    }

    [Fact]
    public void UpdateFrom_WithValidRequest_UpdatesNestedBookingCustomerSupplierClaimDateCompensationFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        claim.UpdateFrom(request);

        // Assert
        Assert.Equal(request.Booking.BookingNumber, claim.Booking.BookingNumber);
        Assert.Equal(request.Booking.SalesChannelId, claim.Booking.SalesChannelId);
        Assert.Equal(request.Booking.SkissimTypeId, claim.Booking.SkissimTypeId);
        Assert.Equal(request.Booking.Product, claim.Booking.Product);

        Assert.Equal(request.Booking.Customer.Name, claim.Booking.Customer.Name);
        Assert.Equal(request.Booking.Customer.AkioNumber, claim.Booking.Customer.AkioNumber);

        Assert.Equal(request.Booking.Supplier.Id, claim.Booking.SupplierId);

        Assert.Equal(request.ClaimDate.DateOfReceivedClaim, claim.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(request.ClaimDate.DateOfStartFollowUp, claim.ClaimDate.DateOfStartFollowUp);
        Assert.Equal(request.ClaimDate.DateLastUpdate, claim.ClaimDate.DateLastUpdate);
        Assert.Equal(request.ClaimDate.DateOfDeparture, claim.ClaimDate.DateOfDeparture);
        Assert.Equal(request.ClaimDate.DateEndOfFollowUp, claim.ClaimDate.DateEndOfFollowUp);
        Assert.Equal(request.ClaimDate.DateOfArrival, claim.ClaimDate.DateOfArrival);

        Assert.Equal(request.Compensation.CustomerVoucher, claim.Compensation.CustomerVoucher);
        Assert.Equal(request.Compensation.CustomerUsedVoucher, claim.Compensation.CustomerUsedVoucher);
        Assert.Equal(request.Compensation.SupplierRefund, claim.Compensation.SupplierRefund);
        Assert.Equal(request.Compensation.ClaimRefund, claim.Compensation.ClaimRefund);
        Assert.Equal(request.Compensation.RefundStateId, claim.Compensation.RefundStateId);
        Assert.Equal(request.Compensation.CompensationReasonId, claim.Compensation.CompensationReasonId);
    }

    [Fact]
    public void UpdateFrom_WithValidRequest_DoesNotChangeEntityIdentitiesOrForeignKeys()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        var originalClaimId = claim.Id;
        var originalBookingId = claim.Booking.Id;
        var originalCustomerId = claim.Booking.Customer.Id;
        var originalClaimDateId = claim.ClaimDate.Id;
        var originalCompensationId = claim.Compensation.Id;

        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        claim.UpdateFrom(request);

        // Assert
        Assert.Equal(originalClaimId, claim.Id);
        Assert.Equal(originalBookingId, claim.Booking.Id);
        Assert.Equal(originalCustomerId, claim.Booking.Customer.Id);
        Assert.Equal(originalClaimDateId, claim.ClaimDate.Id);
        Assert.Equal(originalCompensationId, claim.Compensation.Id);
        Assert.Equal(claim.Booking.Id, claim.BookingId);
        Assert.Equal(claim.Id, claim.ClaimDate.ClaimId);
        Assert.Equal(claim.Id, claim.Compensation.ClaimId);
        Assert.Equal(request.Booking.Supplier.Id, claim.Booking.SupplierId);
    }
}
