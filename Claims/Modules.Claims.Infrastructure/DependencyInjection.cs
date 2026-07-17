using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddClaimsInfrastructure(this IServiceCollection services, IConfiguration configuration, bool enableSensitiveDataLogging)
    {
        var postgresConnectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<ClaimsDbContext>(x =>
        {
            x.UseNpgsql(postgresConnectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(DbConsts.MigrationHistoryTableName, DbConsts.ClaimsSchemaName))
                .UseSnakeCaseNamingConvention();

            if (enableSensitiveDataLogging)
            {
                x.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}
