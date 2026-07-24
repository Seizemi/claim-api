using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public required string BookingNumber { get; set; }
    public SalesChannel? SalesChannel { get; set; }
    public Language? Language { get; set; }
    public BookingService? Service { get; set; }
    public bool? Skissim { get; set; }
    public SkissimType? SkissimType { get; set; }
    public string? Product { get; set; }
    public Claim? Claim { get; set; }
    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public required Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
}
