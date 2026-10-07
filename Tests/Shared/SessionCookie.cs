using ModularMonolith.Authentication;

namespace ModularMonolith.Testing;

public static class SessionCookie
{
    /// <summary>The raw Set-Cookie header of the session cookie, with its attributes, or null.</summary>
    public static string? SetCookieHeader(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(value => value.StartsWith($"{AuthenticationExtensions.SessionCookieName}=", StringComparison.Ordinal))
            : null;

    /// <summary>The session cookie as it is sent back in a Cookie header ("name=value"), or null.</summary>
    public static string? FromResponse(HttpResponseMessage response) =>
        SetCookieHeader(response)?.Split(';')[0];
}
