namespace Modules.Claims.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required int AkioNumber { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
}
