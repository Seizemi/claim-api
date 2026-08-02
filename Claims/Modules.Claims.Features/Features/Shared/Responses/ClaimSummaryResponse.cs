namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record ClaimSummaryResponse(
    Guid Id,
    string CustomerName,
    string Language,
    string BookingNumber,
    string SupplierLabel,
    DateOnly? DateOfStartFollowUp,
    DateOnly? DateOfReceivedClaim,
    int SupplierAkioNumber,
    int CustomerAkioNumber,
    string? FollowedByLabel,
    string? SeasonLabel,
    string? SeasonValue);
