using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Modules.Common.Features.Authorization;

/// <summary>
/// The signed-in user, read from the session cookie's claims. Endpoints take it as a parameter instead of trusting an
/// identity sent by the client. Binding fails with 400 when the session has no valid object ID, which cannot happen
/// for a session issued by Entra.
/// </summary>
public sealed record CurrentUser(Guid Id, string DisplayName)
{
    // Short claim names (MapInboundClaims = false), then the long forms in case inbound mapping is ever turned back on.
    private static readonly string[] ObjectIdClaimTypes = ["oid", "http://schemas.microsoft.com/identity/claims/objectidentifier"];
    private static readonly string[] DisplayNameClaimTypes = ["name", "preferred_username", "email", "emails", ClaimTypes.Email];

    public static ValueTask<CurrentUser?> BindAsync(HttpContext context) => ValueTask.FromResult(FromPrincipal(context.User));

    public static CurrentUser? FromPrincipal(ClaimsPrincipal principal)
    {
        var objectId = FirstValue(principal, ObjectIdClaimTypes);
        if (!Guid.TryParse(objectId, out var id) || id == Guid.Empty)
        {
            return null;
        }

        return new CurrentUser(id, FirstValue(principal, DisplayNameClaimTypes) ?? string.Empty);
    }

    private static string? FirstValue(ClaimsPrincipal principal, string[] claimTypes) =>
        claimTypes.Select(principal.FindFirstValue).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
