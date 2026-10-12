namespace Modules.Users.Features.Features.Shared.Requests;

public sealed record CreateUserRequest(string Email, string DisplayName, string Role);
