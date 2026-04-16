using Microsoft.AspNetCore.Mvc;
using PixelForge.Api.DTOs;
using PixelForge.Application.UseCases;

namespace PixelForge.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IUploadImageUseCase _uploadUseCase;
    private readonly IHttpClientFactory _httpClientFactory;

    public UploadController(IUploadImageUseCase uploadUseCase, IHttpClientFactory httpClientFactory)
    {
        _uploadUseCase = uploadUseCase;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("file")]
    [RequestSizeLimit(50_000_000)] // 50 MB
    public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken token)
    {
        var file = request.file;

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        await using var stream = file.OpenReadStream();
        var filePath = await _uploadUseCase.UploadAsync(stream, file.FileName, request.folder, token);

        return Ok(new { path = filePath });
    }
    [HttpPost("URL")]
    public async Task<IActionResult> Upload(UploadUrlRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest("URL is required.");

        var client = _httpClientFactory.CreateClient();

        // Download from stream
        var response = await client.GetAsync(request.Url, HttpCompletionOption.ResponseHeadersRead, token);

        if (!response.IsSuccessStatusCode)
            return BadRequest("Could not download file from the provided URL.");

        await using var stream = await response.Content.ReadAsStreamAsync(token);

        var fileName = Path.GetFileName(new Uri(request.Url).LocalPath);
        if (string.IsNullOrEmpty(fileName)) fileName = "downloaded_image.jpg";

        var filePath = await _uploadUseCase.UploadAsync(stream, fileName, request.Folder ?? "uploads", token);

        return Ok(new { path = filePath });
    }
}
