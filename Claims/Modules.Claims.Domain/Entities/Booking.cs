namespace Modules.Claims.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public required string BookingNumber { get; set; }
    public required Guid SalesChannelId { get; set; }
    public SalesChannel SalesChannel { get; set; } = null!;
    public required Guid SkissimTypeId { get; set; }
    public SkissimType SkissimType { get; set; } = null!;
    public string? Product { get; set; }
    public Claim? Claim { get; set; }
    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public required Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
}
