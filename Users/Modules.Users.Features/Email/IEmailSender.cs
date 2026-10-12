namespace Modules.Users.Features.Email;

/// <summary>Sends the emails of the Users module. The provider is still to be chosen (spec D2).</summary>
internal interface IEmailSender
{
    Task SendWelcomeAsync(WelcomeEmail email, CancellationToken cancellationToken);
}

/// <param name="To">Address of the new user.</param>
/// <param name="DisplayName">Display name of the new user.</param>
/// <param name="AppUrl">Where the user signs in (<c>App:PublicUrl</c>).</param>
internal sealed record WelcomeEmail(string To, string DisplayName, string AppUrl);
