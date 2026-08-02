namespace Modules.Claims.Domain.Entities;

public class ClaimDate
{
    public Guid Id { get; set; }
    public DateOnly? DateOfReceivedClaim { get; set; }
    public DateOnly? DateOfStartFollowUp { get; set; }
    public DateOnly? DateLastUpdate { get; set; }
    public DateOnly? DateOfDeparture { get; set; }
    public DateOnly? DateEndOfFollowUp { get; set; }
    public DateOnly? DateOfArrival { get; set; }
    public required Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
}
