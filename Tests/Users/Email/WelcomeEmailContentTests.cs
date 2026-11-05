using Modules.Users.Features.Email;
using Xunit;

namespace Modules.Users.Features.Tests.Email;

public sealed class WelcomeEmailContentTests
{
    private const string AppUrl = "https://portal.example.com";

    [Fact]
    public void From_AddressesTheUserInFrenchWithTheLinkAndThePasswordInstructions()
    {
        var content = WelcomeEmailContent.From(new WelcomeEmail("ann@example.com", "Ann Agent", AppUrl));

        Assert.Equal("Votre compte a été créé", content.Subject);
        Assert.Contains("Bonjour Ann Agent,", content.TextBody);
        Assert.Contains(AppUrl, content.TextBody);
        Assert.Contains("Mot de passe oublié ?", content.TextBody);
        Assert.Contains("Bonjour Ann Agent,", content.HtmlBody);
        Assert.Contains($"""<a href="{AppUrl}">""", content.HtmlBody);
        Assert.Contains("Mot de passe oublié ?", content.HtmlBody);
    }

    [Fact]
    public void From_EncodesTheDisplayNameInTheHtmlBody()
    {
        var content = WelcomeEmailContent.From(new WelcomeEmail("ann@example.com", "<b>Ann</b>", AppUrl));

        Assert.DoesNotContain("<b>Ann</b>", content.HtmlBody);
        Assert.Contains("&lt;b&gt;Ann&lt;/b&gt;", content.HtmlBody);
    }
}
