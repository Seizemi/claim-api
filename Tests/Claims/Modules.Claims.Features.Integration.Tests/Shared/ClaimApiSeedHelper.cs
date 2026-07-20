using System.Net.Http.Json;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class ClaimApiSeedHelper
{
    internal static async Task<Guid> SeedClaimAsync(HttpClient client, ClaimRequest? request = null)
    {
        var response = await client.PostAsJsonAsync(RouteConsts.NewClaim, request ?? ClaimRequestFactory.CreateValid(), TestJsonSerializerOptions.Default);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(TestJsonSerializerOptions.Default);
    }
}
