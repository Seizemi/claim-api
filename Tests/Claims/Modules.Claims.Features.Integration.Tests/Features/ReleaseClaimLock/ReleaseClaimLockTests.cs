using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Testing;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.ReleaseClaimLock;

[Collection(IntegrationTestCollection.Name)]
public sealed class ReleaseClaimLockTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ReleaseClaimLock_HeldByCaller_Returns204AndClears()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        await Client.PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        var response = await Client.PostAsync(RouteConsts.ClaimUnlock(claimId), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var claim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        Assert.Null(claim.LockedByUserId);
        Assert.Null(claim.LockedByUserName);
        Assert.Null(claim.LockedAt);
    }

    [Fact]
    public async Task ReleaseClaimLock_NotHeldByCaller_Returns204NoOp()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        await Client.PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        var response = await CreateClient(TestUser.Bob).PostAsync(RouteConsts.ClaimUnlock(claimId), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var claim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        Assert.Equal(TestUser.Alice.Id, claim.LockedByUserId);
    }

    [Fact]
    public async Task ReleaseClaimLock_UnknownClaim_Returns400()
    {
        var response = await Client.PostAsync(RouteConsts.ClaimUnlock(Guid.NewGuid()), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }
}
