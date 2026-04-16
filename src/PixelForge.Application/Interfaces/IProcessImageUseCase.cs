using PixelForge.Application.DTOs;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.UseCases;

public interface IProcessImageUseCase
{
    Task<ImageProcessResult> ProcessAsync(string fullPath, TransformOptions options, CancellationToken token);
}
