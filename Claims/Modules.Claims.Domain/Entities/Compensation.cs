namespace Modules.Claims.Domain.Entities;

public class Compensation
{
    public Guid Id { get; set; }
    public float? CustomerVoucher { get; set; }
    public float? CustomerUsedVoucher { get; set; }
    public float? SupplierRefund { get; set; }
    public float? ClaimRefund { get; set; }
    public required Guid RefundStateId { get; set; }
    public RefundState RefundState { get; set; } = null!;
    public required Guid CompensationReasonId { get; set; }
    public CompensationReason CompensationReason { get; set; } = null!;
    public required Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
}
