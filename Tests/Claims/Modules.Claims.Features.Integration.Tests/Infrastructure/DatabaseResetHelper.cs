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

        context.Claims.RemoveRange(context.Claims);
        context.Bookings.RemoveRange(context.Bookings);
        context.Customers.RemoveRange(context.Customers);
        context.Suppliers.RemoveRange(context.Suppliers);
        context.Users.RemoveRange(context.Users);

        await context.SaveChangesAsync();
    }
}
