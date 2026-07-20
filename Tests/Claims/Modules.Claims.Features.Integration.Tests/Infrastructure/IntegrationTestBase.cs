using Microsoft.Extensions.DependencyInjection;
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
        Client = factory.CreateClient();
        _resetHelper = new DatabaseResetHelper(factory.Services);
    }

    protected HttpClient Client { get; }

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
