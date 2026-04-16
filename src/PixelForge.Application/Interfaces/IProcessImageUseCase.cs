using PixelForge.Application.DTOs;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.UseCases;

public interface IProcessImageUseCase
{
    Task<ImageProcessResult> ProcessImageAsync(string fullPath, TransformOptions options, CancellationToken token);
}
