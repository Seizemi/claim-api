using ModularMonolith.Authentication;

namespace ModularMonolith.Testing;

/// <summary>The CSRF header the React app sends, for test projects that cannot see the host's internal middleware.</summary>
public static class TestCsrf
{
    public const string HeaderName = CsrfHeaderMiddleware.HeaderName;
    public const string HeaderValue = CsrfHeaderMiddleware.HeaderValue;
}
