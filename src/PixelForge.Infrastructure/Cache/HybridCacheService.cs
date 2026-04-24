using Microsoft.Extensions.Caching.Hybrid;
using PixelForge.Application.Interfaces;

namespace PixelForge.Infrastructure.Caching;

public class HybridCacheService : ICacheService
{
    private readonly HybridCache _cache;

    public HybridCacheService(HybridCache cache)
    {
        _cache = cache;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token)
    {
        return await _cache.GetOrCreateAsync(
            key,
            async (ctx) =>
            {
                return await Task.FromResult<byte[]?>(null);
            }, cancellationToken: token);
    }

    public async Task SetAsync(string key, byte[] data, TimeSpan expiration, CancellationToken token)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration
        };

        await _cache.SetAsync(key, data, options, cancellationToken: token);
    }

}
