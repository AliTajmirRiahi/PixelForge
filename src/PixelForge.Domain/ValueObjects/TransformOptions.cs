namespace PixelForge.Domain.ValueObjects;

public sealed class TransformOptions
{
    public uint? Width { get; init; }
    public uint? Height { get; init; }
    public uint? Quality { get; init; }
    public string? Format { get; init; }

    public TransformOptions(uint? width, uint? height, uint? quality, string? format)
    {
        Width = width;
        Height = height;
        Quality = quality;
        Format = format?.ToLower();
    }
}
