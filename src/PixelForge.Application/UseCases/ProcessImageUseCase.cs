using PixelForge.Application.Interfaces;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.UseCases;

public class ProcessImageUseCase : IProcessImageUseCase
{
    private readonly IStorageService _storage;
    private readonly IImageProcessor _processor;
    private readonly ICacheService _cache;

    public ProcessImageUseCase(
        IStorageService storage,
        IImageProcessor processor,
        ICacheService cache)
    {
        _storage = storage;
        _processor = processor;
        _cache = cache;
    }

    public async Task<byte[]> ProcessAsync(string fullPath, TransformOptions options)
    {
        var cacheKey = $"{fullPath}:{options.Width}:{options.Quality}:{options.Format}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
            return cached;

        var inputStream = await _storage.GetAsync(fullPath);
        var outputStream = await _processor.ProcessAsync(inputStream, options);

        using var ms = new MemoryStream();
        await outputStream.CopyToAsync(ms);

        var bytes = ms.ToArray();
        await _cache.SetAsync(cacheKey, bytes, TimeSpan.FromMinutes(10));

        return bytes;
    }
}
