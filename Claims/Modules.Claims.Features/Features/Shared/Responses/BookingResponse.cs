using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record BookingResponse(
    Guid Id,
    string BookingNumber,
    SalesChannel? SalesChannel,
    Language? Language,
    string SeasonLabel,
    string SeasonValue,
    BookingService? Service,
    bool? Skissim,
    SkissimType? SkissimType,
    string? Product,
    CustomerResponse Customer,
    SupplierResponse Supplier);
