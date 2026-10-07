using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Modules.Common.Features.Authorization;
using Xunit;

namespace Modules.Common.Features.Tests.Features.Authorization;

public sealed class CurrentUserTests
{
    private const string ObjectId = "33333333-3333-3333-3333-333333333333";

    [Fact]
    public void FromPrincipal_WithObjectIdAndName_ReturnsUser()
    {
        var principal = Principal(new Claim("oid", ObjectId), new Claim("name", "Alice"));

        var user = CurrentUser.FromPrincipal(principal);

        Assert.Equal(new CurrentUser(Guid.Parse(ObjectId), "Alice"), user);
    }

    [Fact]
    public void FromPrincipal_WithLongObjectIdClaimType_ReturnsUser()
    {
        var principal = Principal(
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", ObjectId),
            new Claim("name", "Alice"));

        var user = CurrentUser.FromPrincipal(principal);

        Assert.NotNull(user);
        Assert.Equal(Guid.Parse(ObjectId), user.Id);
    }

    [Fact]
    public void FromPrincipal_WithoutName_FallsBackToEmail()
    {
        var principal = Principal(new Claim("oid", ObjectId), new Claim("email", "alice@example.com"));

        var user = CurrentUser.FromPrincipal(principal);

        Assert.NotNull(user);
        Assert.Equal("alice@example.com", user.DisplayName);
    }

    [Fact]
    public void FromPrincipal_WithoutNameOrEmail_ReturnsEmptyDisplayName()
    {
        var principal = Principal(new Claim("oid", ObjectId));

        var user = CurrentUser.FromPrincipal(principal);

        Assert.NotNull(user);
        Assert.Equal(string.Empty, user.DisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void FromPrincipal_WithMissingOrInvalidObjectId_ReturnsNull(string? objectId)
    {
        Claim[] claims = objectId is null
            ? [new Claim("name", "Alice")]
            : [new Claim("oid", objectId), new Claim("name", "Alice")];

        var user = CurrentUser.FromPrincipal(Principal(claims));

        Assert.Null(user);
    }

    [Fact]
    public async Task BindAsync_Always_ReadsTheRequestUser()
    {
        var context = new DefaultHttpContext { User = Principal(new Claim("oid", ObjectId), new Claim("name", "Alice")) };

        var user = await CurrentUser.BindAsync(context);

        Assert.Equal(new CurrentUser(Guid.Parse(ObjectId), "Alice"), user);
    }

    [Fact]
    public async Task BindAsync_WithAnonymousUser_ReturnsNull()
    {
        var context = new DefaultHttpContext();

        var user = await CurrentUser.BindAsync(context);

        Assert.Null(user);
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, authenticationType: "Test"));
}
