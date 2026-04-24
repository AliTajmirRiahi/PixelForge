namespace PixelForge.Application.Interfaces;

public interface ICacheService
{
    Task<byte[]?> GetAsync(string ke, CancellationToken token);
    Task SetAsync(string key, byte[] data, TimeSpan expiration, CancellationToken token);
}
