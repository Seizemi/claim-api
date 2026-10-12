namespace Modules.Users.Features.Configuration;

internal sealed class AppSettings
{
    internal const string SectionName = "App";

    /// <summary>Public URL of the application, sent in the welcome email. Slot-specific in Azure.</summary>
    public string PublicUrl { get; init; } = string.Empty;
}
