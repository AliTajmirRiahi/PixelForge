using Microsoft.Extensions.Caching.Hybrid;
using PixelForge.Application.Interfaces;

namespace PixelForge.Infrastructure.Caching;

public class HybridCacheService : ICacheService
{
    private readonly IHybridCacheWrapper _cache;

    public HybridCacheService(IHybridCacheWrapper cache)
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
        await _cache.SetAsync(key, data, expiration, cancellationToken: token);
    }

}
