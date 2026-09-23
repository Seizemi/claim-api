using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.AcquireClaimLock;

[Collection(IntegrationTestCollection.Name)]
public sealed class AcquireClaimLockTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AcquireClaimLock_UnlockedClaim_Returns200Acquired()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);

        var response = await Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(claimId),
            new { userId = Guid.NewGuid(), userName = "Alice" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.True(body!.Acquired);
        Assert.Equal("Alice", body.LockedByUserName);
        Assert.NotNull(body.Claim);
        Assert.Equal(claimId, body.Claim.Id);
        Assert.Equal("Alice", body.Claim.LockedByUserName);
    }

    [Fact]
    public async Task AcquireClaimLock_AlreadyLockedByOther_Returns200NotAcquiredWithHolderInfo()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        await Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(claimId),
            new { userId = Guid.NewGuid(), userName = "Alice" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);

        var response = await Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(claimId),
            new { userId = Guid.NewGuid(), userName = "Bob" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.False(body!.Acquired);
        Assert.Equal("Alice", body.LockedByUserName);
        Assert.Null(body.Claim);
    }

    [Fact]
    public async Task AcquireClaimLock_StaleLock_ReAcquires()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var claim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        claim.LockedByUserId = Guid.NewGuid();
        claim.LockedByUserName = "Alice";
        claim.LockedAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(claimId),
            new { userId = Guid.NewGuid(), userName = "Bob" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.True(body!.Acquired);
        Assert.Equal("Bob", body.LockedByUserName);
        Assert.NotNull(body.Claim);
        Assert.Equal("Bob", body.Claim.LockedByUserName);
    }

    [Fact]
    public async Task AcquireClaimLock_ConcurrentCallers_OnlyOneWins()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);

        var taskA = Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(claimId),
            new { userId = Guid.NewGuid(), userName = "Alice" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);
        var taskB = Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(claimId),
            new { userId = Guid.NewGuid(), userName = "Bob" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);

        var responses = await Task.WhenAll(taskA, taskB);
        var bodies = await Task.WhenAll(responses.Select(r =>
            r.Content.ReadFromJsonAsync<AcquireClaimLockResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken)));

        Assert.Single(bodies, b => b!.Acquired);
        Assert.Single(bodies, b => !b!.Acquired);
    }

    [Fact]
    public async Task AcquireClaimLock_UnknownClaim_Returns400()
    {
        var response = await Client.PostAsJsonAsync(
            RouteConsts.ClaimLock(Guid.NewGuid()),
            new { userId = Guid.NewGuid(), userName = "Alice" },
            TestJsonSerializerOptions.Default,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }
}
