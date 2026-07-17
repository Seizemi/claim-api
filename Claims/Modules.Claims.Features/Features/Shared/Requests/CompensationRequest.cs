using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record CompensationRequest(
    float? CustomerVoucher,
    float? CustomerUsedVoucher,
    float? SupplierRefund,
    float? ClaimRefund,
    RefundState? RefundState);
