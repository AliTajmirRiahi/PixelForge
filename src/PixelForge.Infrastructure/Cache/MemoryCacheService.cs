using Microsoft.Extensions.Caching.Memory;
using PixelForge.Application.Interfaces;

namespace PixelForge.Infrastructure.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token)
    {
        return Task.FromResult(_cache.Get<byte[]>(key));
    }

    public Task SetAsync(string key, byte[] data, TimeSpan expiration, CancellationToken token)
    {
        _cache.Set(key, data, expiration);
        return Task.CompletedTask;
    }
}
