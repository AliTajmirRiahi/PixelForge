namespace PixelForge.Domain.Entities;

public class ImageFile
{
    public string Path { get; }
    public string FileName { get; }
    public string FullPath => $"{Path}/{FileName}".Replace("//", "/");

    public ImageFile(string path, string fileName)
    {
        Path = path;
        FileName = fileName;
    }
}
