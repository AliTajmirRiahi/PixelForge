using System;
using System.Collections.Generic;
using System.Text;

namespace PixelForge.Application.Interfaces
{
    public interface IHybridCacheWrapper
    {
        ValueTask<byte[]?> GetOrCreateAsync(
            string key,
            Func<CancellationToken, ValueTask<byte[]?>> factory,
            CancellationToken cancellationToken);

        ValueTask SetAsync(
            string key,
            byte[] value,
            TimeSpan expiration,
            CancellationToken cancellationToken);
    }

}
