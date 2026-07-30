namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record CompensationReasonResponse(
    Guid Id,
    string Label,
    string Value);
