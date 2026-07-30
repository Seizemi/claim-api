using Modules.Claims.Domain;
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
        Assert.Equal(claim.FollowedBy?.Id, response.FollowedBy?.Id);
        Assert.Equal(claim.Reason.Id, response.Reason.Id);
        Assert.Equal(claim.ClaimSummary, response.ClaimSummary);
        Assert.Equal(claim.Solution.Id, response.Solution.Id);
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
        Assert.Equal(claim.Booking.SalesChannel.Id, response.Booking.SalesChannel.Id);
        Assert.Equal(claim.Booking.Customer.Id, response.Booking.Customer.Id);
        Assert.Equal(claim.Booking.Customer.Name, response.Booking.Customer.Name);
        Assert.Equal(claim.Booking.Customer.AkioNumber, response.Booking.Customer.AkioNumber);
        Assert.Equal(claim.Booking.Supplier.Id, response.Booking.Supplier.Id);
        Assert.Equal(claim.Booking.Supplier.Label, response.Booking.Supplier.Label);
        Assert.Equal(claim.Booking.Supplier.SupplierAkioNumber, response.Booking.Supplier.SupplierAkioNumber);

        Assert.Equal(claim.ClaimDate.Id, response.ClaimDate.Id);
        Assert.Equal(claim.ClaimDate.DateOfReceivedClaim, response.ClaimDate.DateOfReceivedClaim);

        Assert.Equal(claim.Compensation.Id, response.Compensation.Id);
        Assert.Equal(claim.Compensation.RefundState.Id, response.Compensation.RefundState.Id);
        Assert.Equal(claim.Compensation.CompensationReason.Id, response.Compensation.CompensationReason.Id);
    }

    [Fact]
    public void MapToResponse_WithDateOfArrival_ComputesSeasonFromSeasonCalculator()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        claim.ClaimDate.DateOfArrival = new DateTimeOffset(2025, 8, 15, 0, 0, 0, TimeSpan.Zero);
        var (expectedSeasonValue, expectedSeasonLabel) = SeasonCalculator.Compute(claim.ClaimDate.DateOfArrival.Value);

        // Act
        var response = claim.MapToResponse();

        // Assert
        Assert.Equal(expectedSeasonValue, response.Booking.SeasonValue);
        Assert.Equal(expectedSeasonLabel, response.Booking.SeasonLabel);
    }

    [Fact]
    public void MapToResponse_WithNullDateOfArrival_ReturnsNullSeasonFields()
    {
        // Arrange
        var claim = ClaimTestDataFactory.CreateClaim(DateTimeOffset.UtcNow);
        claim.ClaimDate.DateOfArrival = null;

        // Act
        var response = claim.MapToResponse();

        // Assert
        Assert.Null(response.Booking.SeasonValue);
        Assert.Null(response.Booking.SeasonLabel);
    }
}
