namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record ServiceResponse(
    Guid Id,
    string Label,
    string Value);
