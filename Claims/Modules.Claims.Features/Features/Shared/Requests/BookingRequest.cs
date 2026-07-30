using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record BookingRequest(
    string BookingNumber,
    Guid SalesChannelId,
    Language? Language,
    Guid SkissimTypeId,
    string? Product,
    CustomerRequest Customer,
    SupplierRequest Supplier);
