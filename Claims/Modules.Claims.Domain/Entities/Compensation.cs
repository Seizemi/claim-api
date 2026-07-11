using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain.Entities;

public class Compensation
{
    public Guid Id { get; set; }
    public float? CustomerVoucher { get; set; }
    public float? CustomerUsedVoucher { get; set; }
    public float? SupplierRefund { get; set; }
    public float? ClaimRefound { get; set; }
    public RefoundState? RefoundState { get; set; }
    public required Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
}
