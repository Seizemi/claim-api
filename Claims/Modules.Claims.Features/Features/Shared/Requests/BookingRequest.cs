using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record BookingRequest(
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
    CustomerRequest Customer,
    SupplierRequest Supplier);
