namespace Modules.Authentication.Features.Features.Login;

/// <summary>
/// Prevents open redirects after sign-in: only local paths are accepted. Same rules as ASP.NET Core's
/// <c>IUrlHelper.IsLocalUrl</c>, without the app-relative <c>~/</c> form that the SPA never uses.
/// </summary>
internal static class ReturnUrlValidator
{
    internal const string DefaultReturnUrl = "/";

    internal static string GetSafeReturnUrl(string? returnUrl) =>
        IsLocalUrl(returnUrl) ? returnUrl! : DefaultReturnUrl;

    internal static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrEmpty(url) || url[0] != '/')
        {
            return false;
        }

        if (url.Length == 1)
        {
            return true;
        }

        // "//host" and "/\host" are protocol-relative URLs that browsers resolve to another origin.
        if (url[1] is '/' or '\\')
        {
            return false;
        }

        return !url.AsSpan(1).ContainsAnyInRange('\0', '\u001F') && !url.Contains('\u007F');
    }
}
