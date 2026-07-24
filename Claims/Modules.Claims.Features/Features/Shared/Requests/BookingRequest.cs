using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record BookingRequest(
    string BookingNumber,
    SalesChannel? SalesChannel,
    Language? Language,
    BookingService? Service,
    bool? Skissim,
    SkissimType? SkissimType,
    string? Product,
    CustomerRequest Customer,
    SupplierRequest Supplier);
