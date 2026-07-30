namespace Modules.Claims.Domain.Entities;

public class Supplier
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public required string Value { get; set; }
    public required int SupplierAkioNumber { get; set; }
    public required Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
}
