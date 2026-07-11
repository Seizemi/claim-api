namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record SupplierRequest(
    Guid Id,
    string Name,
    int SupplierAkioNumber);
