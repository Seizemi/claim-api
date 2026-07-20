using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.Shared.Mapping;

public sealed class ClaimMappingExtensionsTests
{
    [Fact]
    public void MapToResponse_WithValidClaim_MapsTopLevelFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);

        // Act
        var response = claim.MapToResponse();

        // Assert
        Assert.Equal(claim.Id, response.Id);
        Assert.Equal(claim.State, response.State);
        Assert.Equal(claim.FollowedBy, response.FollowedBy);
        Assert.Equal(claim.Reason, response.Reason);
        Assert.Equal(claim.ClaimSummary, response.ClaimSummary);
        Assert.Equal(claim.Solution, response.Solution);
        Assert.Equal(claim.PurposeOfSolution, response.PurposeOfSolution);
        Assert.Equal(claim.UpdateReason, response.UpdateReason);
        Assert.Equal(claim.CustomerSuppInfo, response.CustomerSuppInfo);
        Assert.Equal(claim.SupplierSuppInfo, response.SupplierSuppInfo);
    }

    [Fact]
    public void MapToResponse_WithValidClaim_MapsNestedBookingCustomerSupplierClaimDateCompensationFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);

        // Act
        var response = claim.MapToResponse();

        // Assert
        Assert.Equal(claim.Booking.Id, response.Booking.Id);
        Assert.Equal(claim.Booking.BookingNumber, response.Booking.BookingNumber);
        Assert.Equal(claim.Booking.SalesChannel, response.Booking.SalesChannel);
        Assert.Equal(claim.Booking.Customer.Id, response.Booking.Customer.Id);
        Assert.Equal(claim.Booking.Customer.Name, response.Booking.Customer.Name);
        Assert.Equal(claim.Booking.Customer.AkioNumber, response.Booking.Customer.AkioNumber);
        Assert.Equal(claim.Booking.Supplier.Id, response.Booking.Supplier.Id);
        Assert.Equal(claim.Booking.Supplier.Name, response.Booking.Supplier.Name);
        Assert.Equal(claim.Booking.Supplier.SupplierAkioNumber, response.Booking.Supplier.SupplierAkioNumber);

        Assert.Equal(claim.ClaimDate.Id, response.ClaimDate.Id);
        Assert.Equal(claim.ClaimDate.DateOfReceivedClaim, response.ClaimDate.DateOfReceivedClaim);

        Assert.Equal(claim.Compensation.Id, response.Compensation.Id);
        Assert.Equal(claim.Compensation.RefundState, response.Compensation.RefundState);
    }
}
