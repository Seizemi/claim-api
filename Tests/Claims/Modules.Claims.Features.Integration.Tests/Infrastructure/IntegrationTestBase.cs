using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Testing;
using Modules.Claims.Infrastructure.Database;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseResetHelper _resetHelper;
    private IServiceScope? _scope;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient(TestUser.Alice);
        _resetHelper = new DatabaseResetHelper(factory.Services);
    }

    /// <summary>Signed in as <see cref="TestUser.Alice"/>, an Agent.</summary>
    protected HttpClient Client { get; }

    protected HttpClient CreateClient(TestUser user) => _factory.CreateClient(user);

    protected HttpClient CreateAnonymousClient() => _factory.CreateAnonymousClient();

    protected ClaimsDbContext DbContext =>
        (_scope ??= _factory.Services.CreateScope())
            .ServiceProvider.GetRequiredService<ClaimsDbContext>();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _scope?.Dispose();
        _scope = null;
        await _resetHelper.ResetAsync();
    }
}
