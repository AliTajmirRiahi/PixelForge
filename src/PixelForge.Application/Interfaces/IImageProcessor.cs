using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.Interfaces;

public interface IImageProcessor
{
    Task<Stream> ProcessAsync(Stream inputImage, TransformOptions options, CancellationToken cancellationToken = default);
}
