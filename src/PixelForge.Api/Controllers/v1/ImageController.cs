using Microsoft.AspNetCore.Mvc;
using PixelForge.Application.Interfaces;
using PixelForge.Application.UseCases;
using PixelForge.Domain.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;

namespace PixelForge.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IProcessImageUseCase _processUseCase;
    private readonly IStorageService _storage;

    public ImageController(IProcessImageUseCase processUseCase, IStorageService storage)
    {
        _processUseCase = processUseCase;
        _storage = storage;
    }

    // دریافت تصویر (raw)
    [HttpGet("{*path}")]
    public async Task<IActionResult> Get(string path, CancellationToken cancellationToken)
    {
        var stream = await _storage.GetAsync(path);
        if (stream == null)
            return NotFound();

        return File(stream, "image/jpeg");
    }


    [HttpGet("process/w{width}/q{quality}/{format?}")]
    public async Task<IActionResult> Process(
        uint width,
        uint quality,
        [FromRoute, SwaggerParameter(Required = false)] string? format,
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Path is required");

        var options = new TransformOptions(width, 0, quality, format);

        var result = await _processUseCase.ProcessImageAsync(path, options, cancellationToken);
        if (result == null)
            return NotFound();

        return File(result.Stream, result.MimeType);
    }



    [HttpGet("process/{format?}")]
    public async Task<IActionResult> Process(
        [FromRoute, SwaggerParameter(Required = false)] string? format,
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Path is required");

        var options = new TransformOptions(0, 0, 0, format);

        var result = await _processUseCase.ProcessImageAsync(path, options, cancellationToken);
        if (result == null)
            return NotFound();

        return File(result.Stream, result.MimeType);
    }

    [HttpGet("thumbnail/q{quality}/{format?}")]
    public async Task<IActionResult> Thumbnail(
        uint quality,
        [FromRoute, SwaggerParameter(Required = false)] string? format,
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Path is required");

        var options = new TransformOptions(0, 0, quality, format);

        var result = await _processUseCase.ThumbnailImageAsync(path, options, cancellationToken);
        if (result == null)
            return NotFound();

        return File(result.Stream, result.MimeType);
    }

}
