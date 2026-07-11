namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record CustomerRequest(
    Guid Id,
    string Name,
    int AkioNumber);
