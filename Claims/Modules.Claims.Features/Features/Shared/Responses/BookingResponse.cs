namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record BookingResponse(
    Guid Id,
    string BookingNumber,
    SalesChannelResponse SalesChannel,
    string? SeasonLabel,
    string? SeasonValue,
    SkissimTypeResponse SkissimType,
    string? Product,
    CustomerResponse Customer,
    SupplierResponse Supplier);
