using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.CreateClaim;

internal static class CreateClaimMappingExtensions
{
    internal static Claim MapToClaim(this ClaimRequest request, Guid id)
    {
        var bookingId = Guid.CreateVersion7();
        return new()
        {
            Id = id,
            State = request.State,
            FollowedById = request.FollowedById,
            ReasonId = request.ReasonId,
            ClaimSummary = request.ClaimSummary,
            SolutionId = request.SolutionId,
            PurposeOfSolution = request.PurposeOfSolution,
            UpdateReason = request.UpdateReason,
            CustomerSuppInfo = request.CustomerSuppInfo,
            SupplierSuppInfo = request.SupplierSuppInfo,
            BookingId = bookingId,
            Booking = request.Booking.MapToBooking(bookingId),
            ClaimDate = request.ClaimDate.MapToClaimDate(id),
            Compensation = request.Compensation.MapToCompensation(id)
        };
    }

    private static Booking MapToBooking(this BookingRequest request, Guid id)
    {
        var customerId = Guid.CreateVersion7();
        return new()
        {
            Id = id,
            BookingNumber = request.BookingNumber,
            SalesChannelId = request.SalesChannelId,
            SkissimTypeId = request.SkissimTypeId,
            Product = request.Product,
            CustomerId = customerId,
            Customer = request.Customer.MapToCustomer(customerId),
            SupplierId = request.Supplier.Id
        };
    }

    private static Customer MapToCustomer(this CustomerRequest request, Guid id) => new()
    {
        Id = id,
        Name = request.Name,
        AkioNumber = request.AkioNumber
    };

    private static ClaimDate MapToClaimDate(this ClaimDateRequest request, Guid claimId) => new()
    {
        Id = Guid.CreateVersion7(),
        ClaimId = claimId,
        DateOfReceivedClaim = request.DateOfReceivedClaim,
        DateOfStartFollowUp = request.DateOfStartFollowUp,
        DateLastUpdate = request.DateLastUpdate,
        DateOfDeparture = request.DateOfDeparture,
        DateEndOfFollowUp = request.DateEndOfFollowUp,
        DateOfArrival = request.DateOfArrival
    };

    private static Compensation MapToCompensation(this CompensationRequest request, Guid claimId) => new()
    {
        Id = Guid.CreateVersion7(),
        ClaimId = claimId,
        CustomerVoucher = request.CustomerVoucher,
        CustomerUsedVoucher = request.CustomerUsedVoucher,
        SupplierRefund = request.SupplierRefund,
        ClaimRefund = request.ClaimRefund,
        RefundStateId = request.RefundStateId,
        CompensationReasonId = request.CompensationReasonId
    };
}
