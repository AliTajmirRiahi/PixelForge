using Microsoft.Extensions.Caching.Hybrid;
using PixelForge.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixelForge.Infrastructure.Cache
{
    public class HybridCacheWrapper : IHybridCacheWrapper
    {
        private readonly HybridCache _cache;

        public HybridCacheWrapper(HybridCache cache)
        {
            _cache = cache;
        }

        public ValueTask<byte[]?> GetOrCreateAsync(
            string key,
            Func<CancellationToken, ValueTask<byte[]?>> factory,
            CancellationToken cancellationToken)
        {
            return _cache.GetOrCreateAsync(
                key,
                factory,
                options: null,
                tags: null,
                cancellationToken: cancellationToken);
        }

        public ValueTask SetAsync(
            string key,
            byte[] value,
            TimeSpan expiration,
            CancellationToken cancellationToken)
        {
            return _cache.SetAsync(
                key,
                value,
                new HybridCacheEntryOptions { Expiration = expiration },
                tags: null,
                cancellationToken: cancellationToken);
        }
    }

}
