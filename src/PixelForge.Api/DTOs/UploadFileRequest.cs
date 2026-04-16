namespace PixelForge.Api.DTOs;

public record UploadFileRequest(IFormFile? file, string folder = "uploads");
