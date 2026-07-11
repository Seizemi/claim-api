using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record ClaimResponse(
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
    BookingResponse Booking,
    ClaimDateResponse ClaimDate,
    CompensationResponse Compensation);
