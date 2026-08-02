namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record BookingRequest(
    string BookingNumber,
    Guid SalesChannelId,
    Guid SkissimTypeId,
    string? Product,
    CustomerRequest Customer,
    SupplierRequest Supplier);
