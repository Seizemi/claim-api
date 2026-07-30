namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record SalesChannelResponse(
    Guid Id,
    string Label,
    string Value,
    string Language);
