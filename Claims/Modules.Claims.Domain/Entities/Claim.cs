using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain.Entities;

public class Claim
{
    public Guid Id { get; set; }
    public required ClaimState State { get; set; }
    public Guid? FollowedById { get; set; }
    public FollowedBy? FollowedBy { get; set; }
    public required Guid ReasonId { get; set; }
    public Reason Reason { get; set; } = null!;
    public string? ClaimSummary { get; set; }
    public required Guid SolutionId { get; set; }
    public Solution Solution { get; set; } = null!;
    public string? PurposeOfSolution { get; set; }
    public string? UpdateReason { get; set; }
    public string? CustomerSuppInfo { get; set; }
    public string? SupplierSuppInfo { get; set; }
    public required Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public ClaimDate ClaimDate { get; set; } = null!;
    public Compensation Compensation { get; set; } = null!;
}
