using Modules.Authentication.Features.Features.Login;
using Xunit;

namespace Modules.Authentication.Features.Tests.Features.Login;

public sealed class ReturnUrlValidatorTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/claims")]
    [InlineData("/claims/42/information?tab=history#top")]
    [InlineData("/admin/users")]
    public void GetSafeReturnUrl_WithLocalPath_ReturnsIt(string returnUrl)
    {
        var result = ReturnUrlValidator.GetSafeReturnUrl(returnUrl);

        Assert.Equal(returnUrl, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://evil.example.com")]
    [InlineData("http://localhost:7001/claims")]
    [InlineData("//evil.example.com")]
    [InlineData("/\\evil.example.com")]
    [InlineData("\\\\evil.example.com")]
    [InlineData("claims")]
    [InlineData("~/claims")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/claims\r\nSet-Cookie: x=1")]
    [InlineData("/\tevil")]
    public void GetSafeReturnUrl_WithNonLocalOrInvalidUrl_ReturnsRoot(string? returnUrl)
    {
        var result = ReturnUrlValidator.GetSafeReturnUrl(returnUrl);

        Assert.Equal(ReturnUrlValidator.DefaultReturnUrl, result);
    }
}
