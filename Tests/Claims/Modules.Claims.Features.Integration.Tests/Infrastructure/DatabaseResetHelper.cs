using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

internal sealed class DatabaseResetHelper(IServiceProvider services)
{
    internal async Task ResetAsync()
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        await context.Claims.ExecuteDeleteAsync();
        await context.Bookings.ExecuteDeleteAsync();
        await context.Customers.ExecuteDeleteAsync();
        await context.Suppliers.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
    }
}
