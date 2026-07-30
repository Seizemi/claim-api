namespace Modules.Claims.Domain.Entities;

public class SalesChannel
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public required string Value { get; set; }
    public required string Language { get; set; }
}
