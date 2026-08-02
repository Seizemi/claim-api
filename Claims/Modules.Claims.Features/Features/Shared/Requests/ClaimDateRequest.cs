namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record ClaimDateRequest(
    DateOnly? DateOfReceivedClaim,
    DateOnly? DateOfStartFollowUp,
    DateOnly? DateLastUpdate,
    DateOnly? DateOfDeparture,
    DateOnly? DateEndOfFollowUp,
    DateOnly? DateOfArrival);
