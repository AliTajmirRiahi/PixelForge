namespace PixelForge.Application.Interfaces;

public interface IStorageService
{
    Task SaveAsync(string path, Stream fileStream, CancellationToken cancellationToken = default);
    Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
}
