using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.GetLookups;

internal interface IGetLookupsHandler : IHandler
{
    Task<LookupsResponse> HandleAsync(CancellationToken cancellationToken);
}

internal sealed class GetLookupsHandler(ClaimsDbContext context, IMemoryCache cache) : IGetLookupsHandler
{
    private const string CacheKey = "Lookups";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(8);
    private static readonly SemaphoreSlim CacheLock = new(1, 1);

    public async Task<LookupsResponse> HandleAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out LookupsResponse? response))
        {
            return response!;
        }

        await CacheLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(CacheKey, out response))
            {
                return response!;
            }

            response = await BuildResponseAsync(cancellationToken);
            cache.Set(CacheKey, response, CacheDuration);
            return response;
        }
        finally
        {
            CacheLock.Release();
        }
    }

    private async Task<LookupsResponse> BuildResponseAsync(CancellationToken cancellationToken) => new(
        (await context.Reasons.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(r => r.MapToResponse()),
        (await context.Solutions.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(s => s.MapToResponse()),
        (await context.FollowedBies.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(f => f.MapToResponse()),
        (await context.RefundStates.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(r => r.MapToResponse()),
        (await context.CompensationReasons.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(r => r.MapToResponse()),
        (await context.SalesChannels.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(s => s.MapToResponse()),
        (await context.Services.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(s => s.MapToResponse()),
        (await context.Suppliers.AsNoTracking().Include(s => s.Service).ToListAsync(cancellationToken)).ConvertAll(s => s.MapToResponse()),
        (await context.SkissimTypes.AsNoTracking().ToListAsync(cancellationToken)).ConvertAll(s => s.MapToResponse()));
}
