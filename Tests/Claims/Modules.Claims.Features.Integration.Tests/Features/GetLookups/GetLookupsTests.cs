using System.Net;
using System.Net.Http.Json;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.GetLookups;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetLookupsTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetLookups_WithSession_Returns200WithEveryList()
    {
        var response = await Client.GetAsync(RouteConsts.Lookups, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lookups = await response.Content.ReadFromJsonAsync<LookupsResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(lookups);
        Assert.Equal(LookupTestIds.ReasonId, Assert.Single(lookups!.Reasons).Id);
        Assert.Equal(LookupTestIds.SolutionId, Assert.Single(lookups.Solutions).Id);
        Assert.Equal(LookupTestIds.FollowedById, Assert.Single(lookups.FollowedBies).Id);
        Assert.Equal(LookupTestIds.RefundStateId, Assert.Single(lookups.RefundStates).Id);
        Assert.Equal(LookupTestIds.CompensationReasonId, Assert.Single(lookups.CompensationReasons).Id);
        Assert.Equal(LookupTestIds.SalesChannelId, Assert.Single(lookups.SalesChannels).Id);
        Assert.Equal(LookupTestIds.ServiceId, Assert.Single(lookups.Services).Id);
        Assert.Equal(LookupTestIds.SkissimTypeId, Assert.Single(lookups.SkissimTypes).Id);

        var supplier = Assert.Single(lookups.Suppliers);
        Assert.Equal(LookupTestIds.SupplierId, supplier.Id);
        Assert.Equal(LookupTestIds.ServiceId, supplier.Service.Id);
    }

    [Fact]
    public async Task GetLookups_WithoutSession_Returns401()
    {
        var response = await CreateAnonymousClient().GetAsync(RouteConsts.Lookups, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
