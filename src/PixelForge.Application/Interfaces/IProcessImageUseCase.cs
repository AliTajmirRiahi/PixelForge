using PixelForge.Domain.ValueObjects;

namespace PixelForge.Application.UseCases;

public interface IProcessImageUseCase
{
    Task<byte[]> ProcessAsync(string fullPath, TransformOptions options);
}
