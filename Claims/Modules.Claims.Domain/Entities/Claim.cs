using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain.Entities;

public class Claim
{
    public Guid Id { get; set; }
    public required ClaimState State { get; set; }
    public string? FollowedBy { get; set; }
    public string? Reason { get; set; }
    public string? ClaimSummary { get; set; }
    public ClaimSolution? Solution { get; set; }
    public string? PurposeOfSolution { get; set; }
    public string? UpdateReason { get; set; }
    public string? CustomerSuppInfo { get; set; }
    public string? SupplierSuppInfo { get; set; }
    public required Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public ClaimDate ClaimDate { get; set; } = null!;
    public Compensation Compensation { get; set; } = null!;
}
