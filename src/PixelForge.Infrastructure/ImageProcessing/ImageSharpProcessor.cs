using ImageMagick;
using PixelForge.Application.DTOs;
using PixelForge.Application.Interfaces;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Infrastructure.ImageProcessing;

public sealed class MagickImageProcessor : IImageProcessor
{
    public async Task<ImageProcessResult> ProcessAsync(
        Stream input,
        TransformOptions options,
        CancellationToken cancellationToken = default)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        input.Position = 0;

        using var image = new MagickImage(input);

        // -------------------------
        // Resize
        // -------------------------
        if (options.Width.HasValue || options.Height.HasValue)
        {
            var width = options.Width;
            var height = options.Height;

            var geometry = new MagickGeometry(
                width ?? 0,
                height ?? 0)
            {
                IgnoreAspectRatio = false
            };

            image.Resize(geometry);
        }

        // -------------------------
        // Quality
        // -------------------------
        if (options.Quality.HasValue)
        {
            image.Quality = options.Quality.Value;
        }

        // -------------------------
        // Format
        // -------------------------
        if (!string.IsNullOrWhiteSpace(options.Format))
        {
            image.Format = ParseFormat(options.Format);
        }

        // -------------------------
        // Output
        // -------------------------
        var output = new MemoryStream();
        await image.WriteAsync(output, image.Format, cancellationToken);
        output.Position = 0;

        var formatInfo = MagickFormatInfo.Create(image.Format);
        var mimeType = formatInfo?.MimeType ?? "image/jpeg";

        return new ImageProcessResult(output, mimeType);
    }

    private static MagickFormat ParseFormat(string format)
    {
        return format.ToLower() switch
        {
            "jpg" or "jpeg" => MagickFormat.Jpeg,
            "png" => MagickFormat.Png,
            "webp" => MagickFormat.WebP,
            "gif" => MagickFormat.Gif,
            _ => MagickFormat.Jpeg
        };
    }

    public string GetImageMimeType(byte[] bytes)
    {
        using var image = new MagickImage(bytes);

        var formatInfo = MagickFormatInfo.Create(image.Format);
        return formatInfo?.MimeType ?? "image/jpeg";

    }
}
