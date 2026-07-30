namespace Modules.Claims.Domain.Entities;

public class FollowedBy
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public required string Value { get; set; }
}
