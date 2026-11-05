using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

// State is nullable so that the validator sees a missing value, which would otherwise bind to the enum's first member.
public sealed record ClaimRequest(
    ClaimState? State,
    Guid? FollowedById,
    Guid ReasonId,
    string? ClaimSummary,
    Guid SolutionId,
    string? PurposeOfSolution,
    string? UpdateReason,
    string? CustomerSuppInfo,
    string? SupplierSuppInfo,
    BookingRequest Booking,
    ClaimDateRequest ClaimDate,
    CompensationRequest Compensation);
