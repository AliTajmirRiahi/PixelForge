using PixelForge.Application.DTOs;
using PixelForge.Application.Interfaces;
using PixelForge.Application.UseCases;
using PixelForge.Domain.ValueObjects;

public class ProcessImageUseCase : IProcessImageUseCase
{
    private readonly IStorageService _storage;
    private readonly ICacheService _cache;
    private readonly IImageProcessor _processor;

    public ProcessImageUseCase(
        IStorageService storage,
        ICacheService cache,
        IImageProcessor processor)
    {
        _storage = storage;
        _cache = cache;
        _processor = processor;
    }

    public async Task<ImageProcessResult> ProcessImageAsync(
        string path,
        TransformOptions options,
        CancellationToken token)
    {
        var cacheKey = $"{path}:{options.Width}:{options.Height}:{options.Quality}:{options.Format}";
        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            return new ImageProcessResult(new MemoryStream(cached), _processor.GetImageMimeType(cached));
        }

        var inputStream = await _storage.GetAsync(path, token);
        if (inputStream == null)
            return null!;

        var result = await _processor.ProcessAsync(inputStream, options, token);

        // save to cache
        using var ms = new MemoryStream();
        await result.Stream.CopyToAsync(ms, token);
        await _cache.SetAsync(cacheKey, ms.ToArray(), TimeSpan.FromMinutes(10));

        result.Stream.Position = 0;
        return result;
    }
}
