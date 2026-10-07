using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record ClaimRequest(
    ClaimState State,
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
