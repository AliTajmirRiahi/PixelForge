using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PixelForge.Api.Controllers.v1;
using PixelForge.Api.DTOs;
using PixelForge.Application.UseCases;
using System.Text;

namespace PixelForge.Unit.Tests.Controllers;

public class UploadControllerTests
{
    private readonly Mock<IUploadImageUseCase> _uploadUseCaseMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    private readonly UploadController _controller;

    public UploadControllerTests()
    {
        _uploadUseCaseMock = new Mock<IUploadImageUseCase>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _controller = new UploadController(
            _uploadUseCaseMock.Object,
            _httpClientFactoryMock.Object);
    }

    // ==============================
    // Upload(file)
    // ==============================

    [Fact]
    public async Task Upload_ShouldReturnBadRequest_WhenFileIsNull()
    {
        var request = new UploadFileRequest(null);

        var result = await _controller.Upload(request, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_ShouldReturnBadRequest_WhenFileIsEmpty()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        var request = new UploadFileRequest(fileMock.Object);

        var result = await _controller.Upload(request, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_ShouldCallUseCase_WhenFileIsValid()
    {
        var content = "test image";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("image.jpg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        _uploadUseCaseMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync("uploads/image.jpg");

        var request = new UploadFileRequest(fileMock.Object, "uploads");

        var result = await _controller.Upload(request, default);

        _uploadUseCaseMock.Verify(
            x => x.UploadAsync(It.IsAny<Stream>(), "image.jpg", "uploads", default),
            Times.Once);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Upload_ShouldReturnPath_WhenUploadSucceeds()
    {
        var content = "filedata";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("photo.png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        _uploadUseCaseMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync("uploads/photo.png");

        var request = new UploadFileRequest(fileMock.Object, "uploads");

        var result = await _controller.Upload(request, default);

        var ok = result as OkObjectResult;

        ok.Should().NotBeNull();
        ok!.Value.Should().NotBeNull();
    }
}
