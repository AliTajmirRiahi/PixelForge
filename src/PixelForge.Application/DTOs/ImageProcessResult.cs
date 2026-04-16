namespace PixelForge.Application.DTOs;

public sealed class ImageProcessResult
{
    public Stream Stream { get; }
    public string MimeType { get; }

    public ImageProcessResult(Stream stream, string mimeType)
    {
        Stream = stream;
        MimeType = mimeType;
    }
}
