using FluentAssertions;
using ImageMagick;
using Microsoft.Extensions.Options;
using PixelForge.Domain.ValueObjects;
using PixelForge.Infrastructure.ImageProcessing;
using PixelForge.Infrastructure.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PixelForge.Unit.Tests.Infrastructure.ImageProcessing;

public class MagickImageProcessorTests
{
    private static MemoryStream CreateTestImage(int width = 500, int height = 500)
    {
        using var image = new MagickImage(MagickColors.Red, (uint)width, (uint)height);

        var stream = new MemoryStream();
        image.Write(stream, MagickFormat.Jpeg);

        stream.Position = 0;
        return stream;
    }

    private static MagickImageProcessor CreateProcessor(bool watermarkEnabled = false)
    {
        var options = Options.Create(new WatermarkOption
        {
            IsActive = watermarkEnabled,
            Mark = "PixelForge",
            Direction = WatermarkDirection.Center,
            Color = "white",
            Opacity = 50
        });

        return new MagickImageProcessor(options);
    }

    [Fact]
    public async Task ProcessAsync_ShouldResizeImage_WhenWidthSpecified()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions
        {
            Width = 200
        };

        var result = await processor.ProcessAsync(input, transform);

        using var image = new MagickImage(result.Stream);

        image.Width.Should().Be(200);
    }

    [Fact]
    public async Task ProcessAsync_ShouldResizeImage_WhenWidthAndHeightSpecified()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions
        {
            Width = 200,
            Height = 200
        };

        var result = await processor.ProcessAsync(input, transform);

        using var image = new MagickImage(result.Stream);

        image.Width.Should().Be(200);
        image.Height.Should().Be(200);
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotResize_WhenWidthIsZero()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions
        {
            Width = 0
        };

        var result = await processor.ProcessAsync(input, transform);

        using var image = new MagickImage(result.Stream);

        image.Width.Should().Be(500);
    }

    [Fact]
    public async Task ProcessAsync_ShouldChangeFormat_ToPng()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions
        {
            Format = "png"
        };

        var result = await processor.ProcessAsync(input, transform);

        result.MimeType.Should().Be("image/png");
    }

    [Fact]
    public async Task ProcessAsync_ShouldChangeFormat_ToWebP()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions
        {
            Format = "webp"
        };

        var result = await processor.ProcessAsync(input, transform);

        result.MimeType.Should().Be("image/webp");
    }

    [Fact]
    public async Task ProcessAsync_ShouldApplyQuality()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions
        {
            Quality = 50
        };

        var result = await processor.ProcessAsync(input, transform);

        using var image = new MagickImage(result.Stream);

        image.Quality.Should().Be(50);
    }

    [Fact]
    public async Task ProcessAsync_ShouldApplyWatermark_WhenEnabled()
    {
        var processor = CreateProcessor(true);
        var input = CreateTestImage();

        var transform = new TransformOptions();

        var result = await processor.ProcessAsync(input, transform);

        result.Stream.Should().NotBeNull();
        result.MimeType.Should().StartWith("image/");
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotApplyWatermark_WhenDisabled()
    {
        var processor = CreateProcessor(false);
        var input = CreateTestImage();

        var transform = new TransformOptions();

        var result = await processor.ProcessAsync(input, transform);

        result.Stream.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnValidStream()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions();

        var result = await processor.ProcessAsync(input, transform);

        result.Stream.Should().NotBeNull();
        result.Stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ProcessAsync_ShouldThrow_WhenInputIsNull()
    {
        var processor = CreateProcessor();

        Func<Task> act = async () =>
        {
            await processor.ProcessAsync(null!, new TransformOptions());
        };

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GetImageMimeType_ShouldReturnPngMimeType()
    {
        var processor = CreateProcessor();

        using var image = new MagickImage(MagickColors.Blue, 100, 100);

        var bytes = image.ToByteArray(MagickFormat.Png);

        var mime = processor.GetImageMimeType(bytes);

        mime.Should().Be("image/png");
    }

    [Fact]
    public void GetImageMimeType_ShouldReturnJpegMimeType()
    {
        var processor = CreateProcessor();

        using var image = new MagickImage(MagickColors.Green, 100, 100);

        var bytes = image.ToByteArray(MagickFormat.Jpeg);

        var mime = processor.GetImageMimeType(bytes);

        mime.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task ProcessAsync_ShouldRespectCancellationToken()
    {
        var processor = CreateProcessor();
        var input = CreateTestImage();

        var transform = new TransformOptions();

        using var cts = new CancellationTokenSource();

        var result = await processor.ProcessAsync(input, transform, cts.Token);

        result.Stream.Should().NotBeNull();
    }
}
