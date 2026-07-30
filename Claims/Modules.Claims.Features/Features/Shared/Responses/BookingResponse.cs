using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record BookingResponse(
    Guid Id,
    string BookingNumber,
    SalesChannelResponse SalesChannel,
    Language? Language,
    string? SeasonLabel,
    string? SeasonValue,
    SkissimTypeResponse SkissimType,
    string? Product,
    CustomerResponse Customer,
    SupplierResponse Supplier);
