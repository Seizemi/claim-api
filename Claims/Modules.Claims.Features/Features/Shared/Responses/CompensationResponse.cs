using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record CompensationResponse(
    Guid Id,
    float? CustomerVoucher,
    float? CustomerUsedVoucher,
    float? SupplierRefund,
    float? ClaimRefund,
    RefundState? RefundState);
