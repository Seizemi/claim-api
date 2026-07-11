namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    int AkioNumber);
