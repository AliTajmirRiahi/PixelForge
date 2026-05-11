using ImageMagick;
using Microsoft.Extensions.Options;
using PixelForge.Application.DTOs;
using PixelForge.Application.Interfaces;
using PixelForge.Domain.ValueObjects;
using PixelForge.Infrastructure.Options;

namespace PixelForge.Infrastructure.ImageProcessing;

public sealed class MagickImageProcessor : IImageProcessor
{
    public IOptions<WatermarkOption> _watermarkOption { get; }

    public MagickImageProcessor(IOptions<WatermarkOption> watermarkOption)
    {
        _watermarkOption = watermarkOption;
    }
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
        if ((options.Width.HasValue || options.Height.HasValue) && options.Width != 0)
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
        if (options.Quality.HasValue && options.Quality != 0)
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
        // Watermark
        // -------------------------
        if (_watermarkOption.Value.IsActive)
        {
            var settings = new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = new MagickColor(_watermarkOption.Value.Color),
                FontPointsize = Math.Clamp(Math.Max(image.Width, image.Height) / 25.0, 12, 120)
            };

            using var watermark = new MagickImage($"label:{_watermarkOption.Value.Mark}", settings);

            if (_watermarkOption.Value.Opacity < 100)
            {
                watermark.Evaluate(
                    Channels.Alpha,
                    EvaluateOperator.Multiply,
                    _watermarkOption.Value.Opacity / 100.0
                );
            }

            // -------------------------
            // Tiled Diagonal Watermark
            // -------------------------
            if (_watermarkOption.Value.Direction == WatermarkDirection.TiledDiagonal)
            {
                watermark.Rotate(45);

                using var tiled = new MagickImage(MagickColors.Transparent, image.Width, image.Height);

                var stepX = (int)watermark.Width * 2;
                var stepY = (int)watermark.Height * 2;

                for (int y = (int)-image.Height; y < image.Height * 2; y += stepY)
                {
                    for (int x = (int)-image.Width; x < image.Width * 2; x += stepX)
                    {
                        tiled.Composite(watermark, x, y, CompositeOperator.Over);
                    }
                }

                image.Composite(tiled, CompositeOperator.Over);
            }
            else
            {
                watermark.Rotate(GetRotation(_watermarkOption.Value.Direction));

                var gravity = GetGravity(_watermarkOption.Value.Direction);

                image.Composite(watermark, gravity, CompositeOperator.Over);
            }
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

    private static Gravity GetGravity(WatermarkDirection direction)
    {
        return direction switch
        {
            WatermarkDirection.Top => Gravity.North,
            WatermarkDirection.Bottom => Gravity.South,
            WatermarkDirection.Left => Gravity.West,
            WatermarkDirection.Right => Gravity.East,
            WatermarkDirection.Center => Gravity.Center,
            WatermarkDirection.TopLeft => Gravity.Northwest,
            WatermarkDirection.TopRight => Gravity.Northeast,
            WatermarkDirection.BottomLeft => Gravity.Southwest,
            WatermarkDirection.BottomRight => Gravity.Southeast,
            _ => Gravity.Center
        };
    }
    private static double GetRotation(WatermarkDirection direction)
    {
        return direction switch
        {
            WatermarkDirection.DiagonalLeft => 45,
            WatermarkDirection.DiagonalRight => -45,
            _ => 0
        };
    }
}
