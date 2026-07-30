namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record FollowedByResponse(
    Guid Id,
    string Label,
    string Value);
