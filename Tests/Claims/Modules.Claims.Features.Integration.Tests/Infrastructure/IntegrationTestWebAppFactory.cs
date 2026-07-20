using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

internal sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("claimapi_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal string ConnectionString => _container.GetConnectionString();

    internal Task InitializeAsync() => _container.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", ConnectionString);
    }
}
