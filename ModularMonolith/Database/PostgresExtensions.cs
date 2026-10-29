using Azure.Core;
using Npgsql;

namespace ModularMonolith.Database;

internal static class PostgresExtensions
{
    internal const string ConnectionStringName = "Postgres";

    // Azure Database for PostgreSQL accepts Entra access tokens for this resource as passwords.
    internal const string EntraTokenScope = "https://ossrdbms-aad.database.windows.net/.default";

    // Tokens last at least an hour and the credential caches them: refreshing often costs nothing and
    // keeps new connections far from the expiry. Open connections are not affected by the expiry.
    private static readonly TimeSpan TokenRefreshInterval = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TokenRetryInterval = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The single PostgreSQL data source (one connection pool) the modules' DbContexts share.
    /// A connection string with a password (local, Docker) is used as is. Without one (Azure), the
    /// password is an Entra token of the managed identity, the connection string's Username being its
    /// PostgreSQL role name.
    /// </summary>
    internal static IServiceCollection AddPostgres(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"The connection string 'ConnectionStrings:{ConnectionStringName}' is missing.");
            }

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            if (string.IsNullOrEmpty(dataSourceBuilder.ConnectionStringBuilder.Password))
            {
                var credential = serviceProvider.GetRequiredService<TokenCredential>();
                dataSourceBuilder.UsePeriodicPasswordProvider(
                    async (_, cancellationToken) =>
                        (await credential.GetTokenAsync(new TokenRequestContext([EntraTokenScope]), cancellationToken)).Token,
                    TokenRefreshInterval,
                    TokenRetryInterval);
            }

            return dataSourceBuilder.Build();
        });

        return services;
    }
}
