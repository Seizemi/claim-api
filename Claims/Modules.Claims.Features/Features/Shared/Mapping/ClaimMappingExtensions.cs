using System.Linq.Expressions;
using Modules.Claims.Domain;
using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Features.Shared.Responses;

namespace Modules.Claims.Features.Features.Shared.Mapping;

internal static class ClaimMappingExtensions
{
    internal static readonly Expression<Func<Claim, ClaimSummaryResponse>> ToSummaryResponse = claim => new ClaimSummaryResponse(
        claim.Id,
        claim.Booking.Customer.Name,
        claim.Booking.SalesChannel.Language,
        claim.Booking.BookingNumber,
        claim.Booking.Supplier.Label,
        claim.ClaimDate.DateOfStartFollowUp,
        claim.ClaimDate.DateOfReceivedClaim,
        claim.Booking.Supplier.SupplierAkioNumber,
        claim.Booking.Customer.AkioNumber,
        claim.FollowedBy != null ? claim.FollowedBy.Label : null,
        claim.ClaimDate.DateOfArrival.HasValue
            ? SeasonCalculator.Compute(claim.ClaimDate.DateOfArrival.Value).SeasonLabel
            : null,
        claim.ClaimDate.DateOfArrival.HasValue
            ? SeasonCalculator.Compute(claim.ClaimDate.DateOfArrival.Value).SeasonValue
            : null);

    internal static ClaimResponse MapToResponse(this Claim claim) => new(
        claim.Id,
        claim.State,
        claim.FollowedBy?.MapToResponse(),
        claim.Reason.MapToResponse(),
        claim.ClaimSummary,
        claim.Solution.MapToResponse(),
        claim.PurposeOfSolution,
        claim.UpdateReason,
        claim.CustomerSuppInfo,
        claim.SupplierSuppInfo,
        claim.Booking.MapToResponse(claim.ClaimDate.DateOfArrival),
        claim.ClaimDate.MapToResponse(),
        claim.Compensation.MapToResponse());

    private static BookingResponse MapToResponse(this Booking booking, DateTimeOffset? dateOfArrival)
    {
        string? seasonValue = null;
        string? seasonLabel = null;
        if (dateOfArrival is not null)
        {
            (seasonValue, seasonLabel) = SeasonCalculator.Compute(dateOfArrival.Value);
        }

        return new(
            booking.Id,
            booking.BookingNumber,
            booking.SalesChannel.MapToResponse(),
            seasonLabel,
            seasonValue,
            booking.SkissimType.MapToResponse(),
            booking.Product,
            booking.Customer.MapToResponse(),
            booking.Supplier.MapToResponse());
    }

    private static CustomerResponse MapToResponse(this Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.AkioNumber);

    internal static SupplierResponse MapToResponse(this Supplier supplier) => new(
        supplier.Id,
        supplier.Label,
        supplier.Value,
        supplier.SupplierAkioNumber,
        supplier.Service.MapToResponse());

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
        compensation.RefundState.MapToResponse(),
        compensation.CompensationReason.MapToResponse());

    internal static ReasonResponse MapToResponse(this Reason reason) => new(
        reason.Id,
        reason.Label,
        reason.Value);

    internal static SolutionResponse MapToResponse(this Solution solution) => new(
        solution.Id,
        solution.Label,
        solution.Value);

    internal static FollowedByResponse MapToResponse(this FollowedBy followedBy) => new(
        followedBy.Id,
        followedBy.Label,
        followedBy.Value);

    internal static RefundStateResponse MapToResponse(this RefundState refundState) => new(
        refundState.Id,
        refundState.Label,
        refundState.Value);

    internal static CompensationReasonResponse MapToResponse(this CompensationReason compensationReason) => new(
        compensationReason.Id,
        compensationReason.Label,
        compensationReason.Value);

    internal static SalesChannelResponse MapToResponse(this SalesChannel salesChannel) => new(
        salesChannel.Id,
        salesChannel.Label,
        salesChannel.Value,
        salesChannel.Language);

    internal static ServiceResponse MapToResponse(this Service service) => new(
        service.Id,
        service.Label,
        service.Value);

    internal static SkissimTypeResponse MapToResponse(this SkissimType skissimType) => new(
        skissimType.Id,
        skissimType.Label,
        skissimType.Value);
}
