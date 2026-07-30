namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record SupplierRequest(
    string Label,
    string Value,
    int SupplierAkioNumber,
    Guid ServiceId);
