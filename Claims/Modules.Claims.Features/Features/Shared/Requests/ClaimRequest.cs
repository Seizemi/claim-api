using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record ClaimRequest(
    Guid Id,
    ClaimState State,
    string? FollowedBy,
    string? Reason,
    string? ClaimSummary,
    ClaimSolution? Solution,
    string? PurposeOfSolution,
    string? UpdateReason,
    string? CustomerSuppInfo,
    string? SupplierSuppInfo,
    BookingRequest Booking,
    ClaimDateRequest ClaimDate,
    CompensationRequest Compensation);
