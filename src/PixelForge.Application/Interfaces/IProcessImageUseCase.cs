using PixelForge.Application.DTOs;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.UseCases;

public interface IProcessImageUseCase
{
    Task<ImageProcessResult> ProcessImageAsync(string path, TransformOptions options, CancellationToken token);
    Task<ImageProcessResult> ThumbnailImageAsync(string path, TransformOptions options, CancellationToken token);
}
