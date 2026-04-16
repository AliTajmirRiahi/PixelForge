using Microsoft.Extensions.Options;
using PixelForge.Application.Interfaces;
using PixelForge.Infrastructure.Options;

namespace PixelForge.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _root;

    public LocalStorageService(IOptions<StorageOptions> options)
    {
        _root = options.Value.LocalRoot
            ?? throw new ArgumentNullException(nameof(options.Value.LocalRoot));
    }

    public async Task SaveAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_root, path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        using var file = File.Create(fullPath);
        await stream.CopyToAsync(file, cancellationToken);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_root, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_root, path);

        if (!ExistsAsync(path, cancellationToken).Result)
            throw new FileNotFoundException("File not found");

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }
}
