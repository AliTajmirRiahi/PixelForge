using PixelForge.Application.DTOs;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.Interfaces;

public interface IImageProcessor
{
    Task<ImageProcessResult> ProcessAsync(Stream input, TransformOptions options, CancellationToken token);
    string GetImageMimeType(byte[] bytes);
}
