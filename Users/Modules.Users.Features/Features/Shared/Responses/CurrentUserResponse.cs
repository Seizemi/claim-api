namespace Modules.Users.Features.Features.Shared.Responses;

public sealed record CurrentUserResponse(string Id, string DisplayName, string Email, IReadOnlyList<string> Roles);
