namespace Modules.Claims.Domain.Entities;

public class ClaimDate
{
    public Guid Id { get; set; }
    public DateTimeOffset? DateOfReceivedClaim { get; set; }
    public DateTimeOffset? DateOfStartFollowUp { get; set; }
    public DateTimeOffset? DateLastUpdate { get; set; }
    public DateTimeOffset? DateOfDeparture { get; set; }
    public DateTimeOffset? DateEndOfFollowUp { get; set; }
    public DateTimeOffset? DateOfArrival { get; set; }
    public required Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
}
