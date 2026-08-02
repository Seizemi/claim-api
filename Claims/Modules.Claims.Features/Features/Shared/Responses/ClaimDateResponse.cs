namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record ClaimDateResponse(
    Guid Id,
    DateOnly? DateOfReceivedClaim,
    DateOnly? DateOfStartFollowUp,
    DateOnly? DateLastUpdate,
    DateOnly? DateOfDeparture,
    DateOnly? DateEndOfFollowUp,
    DateOnly? DateOfArrival);
