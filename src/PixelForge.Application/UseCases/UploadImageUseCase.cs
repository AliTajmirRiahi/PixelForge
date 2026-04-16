using PixelForge.Application.Interfaces;

namespace PixelForge.Application.UseCases;

public class UploadImageUseCase : IUploadImageUseCase
{
    private readonly IStorageService _storage;

    public UploadImageUseCase(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken token)
    {
        var fullPath = $"{folder}/{fileName}".Replace("//", "/");
        await _storage.SaveAsync(fullPath, fileStream, token);
        return fullPath;
    }
}
