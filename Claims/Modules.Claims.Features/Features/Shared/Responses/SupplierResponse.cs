namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record SupplierResponse(
    Guid Id,
    string Name,
    int SupplierAkioNumber);
