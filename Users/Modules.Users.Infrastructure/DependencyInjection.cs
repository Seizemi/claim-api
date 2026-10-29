using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Infrastructure.Database;
using Npgsql;

namespace Modules.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services)
    {
        // The data source (connection string, Entra token in Azure) is registered by the host.
        services.AddDbContext<UsersDbContext>((serviceProvider, x) => x
            .UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>(), npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(DbConsts.MigrationHistoryTableName, DbConsts.UsersSchemaName))
            .UseSnakeCaseNamingConvention());

        services.AddHealthChecks()
            .AddDbContextCheck<UsersDbContext>();

        return services;
    }
}
