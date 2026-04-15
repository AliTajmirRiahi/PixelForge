using PixelForge.Application.Interfaces;
using PixelForge.Domain.ValueObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PixelForge.Infrastructure.ImageProcessing;

public class ImageSharpProcessor : IImageProcessor
{
    public async Task<Stream> ProcessAsync(Stream input, TransformOptions options, CancellationToken cancellationToken = default)
    {
        input.Position = 0;
        using var image = await Image.LoadAsync(input, cancellationToken);

        if (options.Width.HasValue)
        {
            image.Mutate(x => x.Resize(options.Width.Value, 0));
        }

        var output = new MemoryStream();
        await image.SaveAsJpegAsync(output, cancellationToken: cancellationToken);
        output.Position = 0;

        return output;
    }
}
