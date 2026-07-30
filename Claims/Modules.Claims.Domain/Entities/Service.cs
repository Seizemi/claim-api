namespace Modules.Claims.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public required string Value { get; set; }
    public ICollection<Supplier> Suppliers { get; set; } = [];
}
