using ClaimPartners.Users.Configuration;

namespace Modules.Users.Features.Configuration;

/// <summary>Adapts the host's callback to the package's contract, which the host can't see (PrivateAssets compile).</summary>
internal sealed class GraphClientAssertionProvider(GraphClientAssertionCallback callback) : IGraphClientAssertionProvider
{
    public Task<string> GetAssertionAsync(string clientId, string audience, CancellationToken cancellationToken) =>
        callback(clientId, audience, cancellationToken);
}
