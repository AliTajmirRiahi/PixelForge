namespace PixelForge.Domain.ValueObjects;

public class TransformOptions
{
    public int? Width { get; init; }
    public int? Quality { get; init; }
    public string? Format { get; init; }

    public TransformOptions(int? width, int? quality, string? format)
    {
        Width = width;
        Quality = quality;
        Format = format?.ToLower();
    }
}
