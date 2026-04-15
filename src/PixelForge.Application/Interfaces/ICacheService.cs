namespace PixelForge.Application.Interfaces;

public interface ICacheService
{
    Task<byte[]?> GetAsync(string key);
    Task SetAsync(string key, byte[] data, TimeSpan expiration);
}
