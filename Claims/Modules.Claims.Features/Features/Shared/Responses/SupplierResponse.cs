namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record SupplierResponse(
    Guid Id,
    string Label,
    string Value,
    int SupplierAkioNumber,
    ServiceResponse Service);
