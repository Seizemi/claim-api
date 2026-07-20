using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;

namespace Modules.Claims.Features.Integration.Tests.Features.GetAllClaims;

[TestClass]
public sealed class GetAllClaimsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAllClaims_MultipleSeededClaims_ReturnsPagedResponseOrderedByReceivedDateDescending()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var oldestId = await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate));
        var middleId = await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate.AddDays(1)));
        var newestId = await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate.AddDays(2)));

        var response = await Client.GetAsync(RouteConsts.DashboardClaim);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(paged);
        Assert.AreEqual(3, paged!.TotalCount);
        Assert.AreEqual(1, paged.TotalPages);
        CollectionAssert.AreEqual(
            new[] { newestId, middleId, oldestId },
            paged.Items.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public async Task GetAllClaims_PageSizeSmallerThanTotal_ReturnsCorrectPageAndTotalPages()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var ids = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            ids.Add(await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate.AddDays(i))));
        }

        // Newest first: ids[4], ids[3], ids[2], ids[1], ids[0]. Page 2 of size 2 -> ids[2], ids[1].
        var response = await Client.GetAsync($"{RouteConsts.DashboardClaim}?PageNumber=2&PageSize=2");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(paged);
        Assert.AreEqual(5, paged!.TotalCount);
        Assert.AreEqual(3, paged.TotalPages);
        Assert.AreEqual(2, paged.PageNumber);
        Assert.AreEqual(2, paged.PageSize);
        CollectionAssert.AreEqual(
            new[] { ids[2], ids[1] },
            paged.Items.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public async Task GetAllClaims_PageSizeZero_Returns400ValidationProblem()
    {
        var response = await Client.GetAsync($"{RouteConsts.DashboardClaim}?PageSize=0");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("PageSize"));
    }

    [TestMethod]
    public async Task GetAllClaims_PageSizeAboveMax_Returns400ValidationProblem()
    {
        var response = await Client.GetAsync($"{RouteConsts.DashboardClaim}?PageSize=101");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("PageSize"));
    }

    [TestMethod]
    public async Task GetAllClaims_PageNumberZero_Returns400ValidationProblem()
    {
        var response = await Client.GetAsync($"{RouteConsts.DashboardClaim}?PageNumber=0");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("PageNumber"));
    }

    [TestMethod]
    public async Task GetAllClaims_NoClaims_ReturnsEmptyPagedResponse()
    {
        var response = await Client.GetAsync(RouteConsts.DashboardClaim);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(paged);
        Assert.AreEqual(0, paged!.TotalCount);
        Assert.AreEqual(0, paged.TotalPages);
        Assert.AreEqual(0, paged.Items.Count);
    }

    [TestMethod]
    public async Task GetAllClaims_PageNumberBeyondAvailablePages_ReturnsEmptyItemsWithCorrectTotals()
    {
        await ClaimApiSeedHelper.SeedClaimAsync(Client);

        var response = await Client.GetAsync($"{RouteConsts.DashboardClaim}?PageNumber=99&PageSize=10");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(paged);
        Assert.AreEqual(1, paged!.TotalCount);
        Assert.AreEqual(1, paged.TotalPages);
        Assert.AreEqual(99, paged.PageNumber);
        Assert.AreEqual(0, paged.Items.Count);
    }

    [TestMethod]
    public async Task GetAllClaims_NonNumericPageSize_Returns400BadRequest()
    {
        var response = await Client.GetAsync($"{RouteConsts.DashboardClaim}?PageSize=abc");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.AreEqual("Bad request", problem!.Title);
    }
}
