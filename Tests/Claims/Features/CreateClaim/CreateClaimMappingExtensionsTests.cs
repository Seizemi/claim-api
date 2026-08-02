using Modules.Claims.Features.Features.CreateClaim;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.CreateClaim;

public sealed class CreateClaimMappingExtensionsTests
{
    [Fact]
    public void MapToClaim_WithValidRequest_MapsTopLevelClaimFields()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        var claimId = Guid.CreateVersion7();

        // Act
        var claim = request.MapToClaim(claimId);

        // Assert
        Assert.Equal(claimId, claim.Id);
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
    public void MapToClaim_WithValidRequest_MapsNestedBookingCustomerSupplierClaimDateCompensationFields()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        var claimId = Guid.CreateVersion7();

        // Act
        var claim = request.MapToClaim(claimId);

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
    public void MapToClaim_WithValidRequest_LinksForeignKeysAndAssignsDistinctNonEmptyIds()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        var claimId = Guid.CreateVersion7();

        // Act
        var claim = request.MapToClaim(claimId);

        // Assert
        Assert.Equal(claim.Booking.Id, claim.BookingId);
        Assert.Equal(claim.Booking.Customer.Id, claim.Booking.CustomerId);
        Assert.Equal(request.Booking.Supplier.Id, claim.Booking.SupplierId);
        Assert.Equal(claim.Id, claim.ClaimDate.ClaimId);
        Assert.Equal(claim.Id, claim.Compensation.ClaimId);

        var ids = new[]
        {
            claim.Id,
            claim.Booking.Id,
            claim.Booking.Customer.Id,
            claim.ClaimDate.Id,
            claim.Compensation.Id
        };

        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, ids);
    }
}
