using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Database;
using Npgsql;
using Xunit;

namespace ModularMonolith.Tests.Database;

public sealed class PostgresTests
{
    [Fact]
    public void AddPostgres_WithAPassword_UsesTheConnectionStringAsIs()
    {
        // No TokenCredential registered: resolving it would throw.
        using var provider = BuildProvider("Host=localhost;Database=claimapi;Username=postgres;Password=postgres", credential: null);

        var dataSource = provider.GetRequiredService<NpgsqlDataSource>();

        Assert.Equal("postgres", new NpgsqlConnectionStringBuilder(dataSource.ConnectionString).Username);
    }

    [Fact]
    public async Task AddPostgres_WithoutAPassword_RequestsAnEntraTokenForPostgreSql()
    {
        var credential = new RecordingCredential();
        using var provider = BuildProvider("Host=psql-claim.postgres.database.azure.com;Database=claimapi;Username=app-claim;Ssl Mode=Require", credential);

        // The periodic password provider fetches the first token as soon as the data source is built.
        _ = provider.GetRequiredService<NpgsqlDataSource>();
        var scopes = await credential.FirstRequest.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.Equal([PostgresExtensions.EntraTokenScope], scopes);
    }

    [Fact]
    public void AddPostgres_WithoutConnectionString_FailsWithAClearMessage()
    {
        using var provider = BuildProvider(connectionString: null, credential: null);

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<NpgsqlDataSource>());

        Assert.Contains("ConnectionStrings:Postgres", exception.Message);
    }

    private static ServiceProvider BuildProvider(string? connectionString, TokenCredential? credential)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"ConnectionStrings:{PostgresExtensions.ConnectionStringName}"] = connectionString })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        if (credential is not null)
        {
            services.AddSingleton(credential);
        }

        services.AddPostgres();

        return services.BuildServiceProvider();
    }

    private sealed class RecordingCredential : TokenCredential
    {
        private readonly TaskCompletionSource<string[]> _firstRequest = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal Task<string[]> FirstRequest => _firstRequest.Task;

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            _firstRequest.TrySetResult(requestContext.Scopes);
            return new AccessToken("entra-token", DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            ValueTask.FromResult(GetToken(requestContext, cancellationToken));
    }
}
