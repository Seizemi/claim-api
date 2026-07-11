namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record ClaimDateRequest(
    DateTimeOffset? DateOfReceivedClaim,
    DateTimeOffset? DateOfStartFollowUp,
    DateTimeOffset? DateLastUpdate,
    DateTimeOffset? DateOfDeparture,
    DateTimeOffset? DateEndOfFollowUp,
    DateTimeOffset? DateOfArrival);
