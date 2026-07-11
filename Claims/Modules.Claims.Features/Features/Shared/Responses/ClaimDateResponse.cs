namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record ClaimDateResponse(
    Guid Id,
    DateTimeOffset? DateOfReceivedClaim,
    DateTimeOffset? DateOfStartFollowUp,
    DateTimeOffset? DateLastUpdate,
    DateTimeOffset? DateOfDeparture,
    DateTimeOffset? DateEndOfFollowUp,
    DateTimeOffset? DateOfArrival);
