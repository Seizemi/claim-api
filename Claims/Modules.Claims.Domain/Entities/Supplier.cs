namespace Modules.Claims.Domain.Entities;

public class Supplier
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required int SupplierAkioNumber { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
}
