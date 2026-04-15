namespace PixelForge.Application.UseCases;

public interface IUploadImageUseCase
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string folder);
}
