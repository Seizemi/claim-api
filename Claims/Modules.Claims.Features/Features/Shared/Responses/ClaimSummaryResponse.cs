using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record ClaimSummaryResponse(
    Guid Id,
    string CustomerName,
    Language? Language,
    string BookingNumber,
    string SupplierLabel,
    DateTimeOffset? DateOfStartFollowUp,
    DateTimeOffset? DateOfReceivedClaim,
    int SupplierAkioNumber,
    int CustomerAkioNumber,
    string? FollowedByLabel);
