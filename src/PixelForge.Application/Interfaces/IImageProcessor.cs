using PixelForge.Application.DTOs;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.Interfaces;

public interface IImageProcessor
{
    Task<ImageProcessResult> ProcessAsync(Stream input, TransformOptions options, CancellationToken cancellationToken);
    Task<ImageProcessResult> ThumbnailAsync(Stream input, TransformOptions options, CancellationToken cancellationToken);
    string GetImageMimeType(byte[] bytes);
}
