using Microsoft.AspNetCore.Mvc;
using Moq;
using PixelForge.Api.Controllers;
using PixelForge.Application.DTOs;
using PixelForge.Application.Interfaces;
using PixelForge.Application.UseCases;
using PixelForge.Domain.ValueObjects;

namespace PixelForge.Unit.Tests.Controllers;

public class ImageControllerTests
{
    private readonly Mock<IStorageService> _storageMock;
    private readonly Mock<IProcessImageUseCase> _processMock;
    private readonly ImageController _controller;

    public ImageControllerTests()
    {
        _storageMock = new Mock<IStorageService>();
        _processMock = new Mock<IProcessImageUseCase>();

        _controller = new ImageController(_processMock.Object, _storageMock.Object);
    }

    // -------------------------------------------------------------
    // GET RAW IMAGE
    // -------------------------------------------------------------

    [Fact]
    public async Task Get_ShouldReturnFile_WhenImageExists()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        _storageMock.Setup(s => s.GetAsync("images/cat.jpg"))
                    .ReturnsAsync(stream);

        var result = await _controller.Get("images/cat.jpg", CancellationToken.None);

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/jpeg", fileResult.ContentType);
        Assert.Equal(stream, fileResult.FileStream);
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        _storageMock.Setup(s => s.GetAsync("missing.jpg", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Stream?)null);

        var result = await _controller.Get("missing.jpg", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }


    // -------------------------------------------------------------
    // PROCESS: w + q + format
    // -------------------------------------------------------------

    [Fact]
    public async Task Process_WithWidthQualityFormat_ShouldReturnFile()
    {
        var fakeResult = new ImageProcessResult(new MemoryStream(new byte[] { 10 }), "image/png");

        _processMock.Setup(x => x.ProcessImageAsync("img.jpg",
            It.IsAny<TransformOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResult);

        var result = await _controller.Process(
            width: 500,
            quality: 80,
            format: "png",
            path: "img.jpg",
            cancellationToken: CancellationToken.None);

        var file = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/png", file.ContentType);
        Assert.NotNull(file.FileStream);

        _processMock.Verify(x =>
            x.ProcessImageAsync("img.jpg",
                It.Is<TransformOptions>(o =>
                    o.Width == 500 &&
                    o.Height == 0 &&
                    o.Quality == 80 &&
                    o.Format == "png"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Process_WithWidthQualityFormat_ShouldReturnNotFound_WhenUseCaseReturnsNull()
    {
        _processMock.Setup(x => x.ProcessImageAsync(It.IsAny<string>(), It.IsAny<TransformOptions>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(default(ImageProcessResult));

        var result = await _controller.Process(200, 70, "jpg", "path/to/file", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Process_WithWidthQualityFormat_ShouldReturnBadRequest_WhenPathMissing()
    {
        var result = await _controller.Process(100, 60, "png", "", default);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Path is required", bad.Value);
    }


    // -------------------------------------------------------------
    // PROCESS: format only (optional)
    // -------------------------------------------------------------

    [Fact]
    public async Task Process_OptionalFormat_ShouldReturnFile()
    {
        var fakeResult = new ImageProcessResult(new MemoryStream(new byte[] { 5 }), "image/webp");

        _processMock.Setup(x => x.ProcessImageAsync("img.jpg", It.IsAny<TransformOptions>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(fakeResult);

        var result = await _controller.Process(
            format: "webp",
            path: "img.jpg",
            cancellationToken: CancellationToken.None);

        var file = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/webp", file.ContentType);

        _processMock.Verify(x =>
            x.ProcessImageAsync("img.jpg",
                It.Is<TransformOptions>(o =>
                    o.Width == 0 &&
                    o.Height == 0 &&
                    o.Quality == 0 &&
                    o.Format == "webp"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Process_OptionalFormat_ShouldAllowNullFormat()
    {
        var fakeResult = new ImageProcessResult(new MemoryStream(new byte[] { 2 }), "image/jpeg");

        _processMock.Setup(x => x.ProcessImageAsync("img.jpg", It.IsAny<TransformOptions>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(fakeResult);

        var result = await _controller.Process(
            format: null,
            path: "img.jpg",
            cancellationToken: CancellationToken.None);

        Assert.IsType<FileStreamResult>(result);

        _processMock.Verify(x =>
            x.ProcessImageAsync("img.jpg",
                It.Is<TransformOptions>(o => o.Format == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Process_OptionalFormat_ShouldReturnNotFound_WhenUseCaseReturnsNull()
    {
        _processMock.Setup(x => x.ProcessImageAsync(It.IsAny<string>(), It.IsAny<TransformOptions>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ImageProcessResult?)null);

        var result = await _controller.Process(
            format: "png",
            path: "missing.jpg",
            cancellationToken: CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Process_OptionalFormat_ShouldReturnBadRequest_WhenPathMissing()
    {
        var result = await _controller.Process(
            format: "jpg",
            path: "",
            cancellationToken: CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Path is required", bad.Value);
    }


    // -------------------------------------------------------------
    // CancellationToken Behavior
    // -------------------------------------------------------------

    [Fact]
    public async Task Process_ShouldPassCancellationTokenToUseCase()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        var fake = new ImageProcessResult(new MemoryStream(), "image/jpeg");
        _processMock.Setup(x => x.ProcessImageAsync(It.IsAny<string>(), It.IsAny<TransformOptions>(), token))
                    .ReturnsAsync(fake);

        await _controller.Process(width: 100, quality: 50, format: "jpg", path: "a.jpg", cancellationToken: token);

        _processMock.Verify(x =>
            x.ProcessImageAsync("a.jpg", It.IsAny<TransformOptions>(), token),
            Times.Once);
    }
}
