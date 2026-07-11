using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required UserRole Role { get; set; }
}
