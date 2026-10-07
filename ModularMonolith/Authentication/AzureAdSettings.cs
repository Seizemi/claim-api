namespace ModularMonolith.Authentication;

/// <summary>
/// The subset of the AzureAd section that must be present for sign-in to work. Microsoft.Identity.Web binds the
/// full section itself; this class only exists to fail at startup with a clear message when a value is missing.
/// </summary>
internal sealed class AzureAdSettings
{
    internal const string SectionName = "AzureAd";

    public string? Authority { get; init; }
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}
