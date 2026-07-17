using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Features.Shared.Responses;

namespace Modules.Claims.Features.Features.Shared.Mapping;

internal static class ClaimMappingExtensions
{
    internal static ClaimResponse MapToResponse(this Claim claim) => new(
        claim.Id,
        claim.State,
        claim.FollowedBy,
        claim.Reason,
        claim.ClaimSummary,
        claim.Solution,
        claim.PurposeOfSolution,
        claim.UpdateReason,
        claim.CustomerSuppInfo,
        claim.SupplierSuppInfo,
        claim.Booking.MapToResponse(),
        claim.ClaimDate.MapToResponse(),
        claim.Compensation.MapToResponse());

    private static BookingResponse MapToResponse(this Booking booking) => new(
        booking.Id,
        booking.BookingNumber,
        booking.SalesChannel,
        booking.Language,
        booking.SeasonLabel,
        booking.SeasonValue,
        booking.Service,
        booking.Skissim,
        booking.SkissimType,
        booking.Product,
        booking.Customer.MapToResponse(),
        booking.Supplier.MapToResponse());

    private static CustomerResponse MapToResponse(this Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.AkioNumber);

    private static SupplierResponse MapToResponse(this Supplier supplier) => new(
        supplier.Id,
        supplier.Name,
        supplier.SupplierAkioNumber);

    private static ClaimDateResponse MapToResponse(this ClaimDate claimDate) => new(
        claimDate.Id,
        claimDate.DateOfReceivedClaim,
        claimDate.DateOfStartFollowUp,
        claimDate.DateLastUpdate,
        claimDate.DateOfDeparture,
        claimDate.DateEndOfFollowUp,
        claimDate.DateOfArrival);

    private static CompensationResponse MapToResponse(this Compensation compensation) => new(
        compensation.Id,
        compensation.CustomerVoucher,
        compensation.CustomerUsedVoucher,
        compensation.SupplierRefund,
        compensation.ClaimRefund,
        compensation.RefundState);
}
