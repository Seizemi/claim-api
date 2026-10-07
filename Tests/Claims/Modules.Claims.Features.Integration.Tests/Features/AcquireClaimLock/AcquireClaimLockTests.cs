using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Testing;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.AcquireClaimLock;

[Collection(IntegrationTestCollection.Name)]
public sealed class AcquireClaimLockTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AcquireClaimLock_UnlockedClaim_Returns200AcquiredBySessionUser()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);

        var response = await Client.PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.True(body!.Acquired);
        Assert.Equal(TestUser.Alice.Id, body.LockedByUserId);
        Assert.Equal(TestUser.Alice.Name, body.LockedByUserName);
        Assert.NotNull(body.Claim);
        Assert.Equal(claimId, body.Claim.Id);
        Assert.Equal(TestUser.Alice.Name, body.Claim.LockedByUserName);
    }

    [Fact]
    public async Task AcquireClaimLock_AlreadyLockedByOther_Returns200NotAcquiredWithHolderInfo()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        await Client.PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        var response = await CreateClient(TestUser.Bob).PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.False(body!.Acquired);
        Assert.Equal(TestUser.Alice.Name, body.LockedByUserName);
        Assert.Null(body.Claim);
    }

    [Fact]
    public async Task AcquireClaimLock_StaleLock_ReAcquires()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var claim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        claim.LockedByUserId = TestUser.Alice.Id;
        claim.LockedByUserName = TestUser.Alice.Name;
        claim.LockedAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await CreateClient(TestUser.Bob).PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.True(body!.Acquired);
        Assert.Equal(TestUser.Bob.Name, body.LockedByUserName);
        Assert.NotNull(body.Claim);
        Assert.Equal(TestUser.Bob.Name, body.Claim.LockedByUserName);
    }

    [Fact]
    public async Task AcquireClaimLock_ConcurrentCallers_OnlyOneWins()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);

        var taskA = Client.PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);
        var taskB = CreateClient(TestUser.Bob).PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        var responses = await Task.WhenAll(taskA, taskB);
        var bodies = await Task.WhenAll(responses.Select(r =>
            r.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken)));

        Assert.Single(bodies, b => b!.Acquired);
        Assert.Single(bodies, b => !b!.Acquired);
    }

    [Fact]
    public async Task AcquireClaimLock_UnknownClaim_Returns400()
    {
        var response = await Client.PostAsync(RouteConsts.ClaimLock(Guid.NewGuid()), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }

    [Fact]
    public async Task AcquireClaimLock_WithoutSession_Returns401()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var response = await CreateAnonymousClient().PostAsync(RouteConsts.ClaimLock(claimId), content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var claim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        Assert.Null(claim.LockedByUserId);
    }
}
