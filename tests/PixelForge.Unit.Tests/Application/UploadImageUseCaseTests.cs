using FluentAssertions;
using Moq;
using PixelForge.Application.Interfaces;
using PixelForge.Application.UseCases;

namespace PixelForge.Unit.Tests.Application;

public class UploadImageUseCaseTests
{
    private readonly Mock<IStorageService> _storage = new();
    private readonly UploadImageUseCase _useCase;

    public UploadImageUseCaseTests()
    {
        _useCase = new UploadImageUseCase(_storage.Object);
    }

    // --------------------------------------------------------------------
    // TEST 1
    // Ensure fullPath is created correctly with Replace("//", "/")
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Create_Correct_FullPath()
    {
        var inputStream = new MemoryStream(new byte[] { 1 });

        string folder = "images/";
        string fileName = "cat.png";

        string? capturedPath = null;
        Stream? capturedStream = null;

        _storage
            .Setup(s => s.SaveAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Stream, CancellationToken>((p, s, _) =>
            {
                capturedPath = p;
                capturedStream = s;
            })
            .Returns(Task.CompletedTask);

        var result = await _useCase.UploadAsync(inputStream, fileName, folder, default);

        result.Should().Be("images/cat.png");
        capturedPath.Should().Be("images/cat.png");
        capturedStream.Should().BeSameAs(inputStream);
    }

    // --------------------------------------------------------------------
    // TEST 2
    // Ensure SaveAsync is called exactly once with correct parameters
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Call_SaveAsync_Once()
    {
        var stream = new MemoryStream();
        var folder = "uploads";
        var file = "a.jpg";

        await _useCase.UploadAsync(stream, file, folder, default);

        _storage.Verify(
            s => s.SaveAsync("uploads/a.jpg", stream),
            Times.Once);
    }

    // --------------------------------------------------------------------
    // TEST 3
    // If Storage.SaveAsync throws, the exception must bubble up
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Throw_When_Storage_Save_Fails()
    {
        var stream = new MemoryStream();
        var folder = "err";
        var file = "x.png";

        _storage
            .Setup(s => s.SaveAsync("err/x.png", stream))
            .ThrowsAsync(new IOException("save failed"));

        Func<Task> act = () => _useCase.UploadAsync(stream, file, folder, default);

        await act.Should().ThrowAsync<IOException>()
                 .WithMessage("save failed");

        _storage.Verify(
            s => s.SaveAsync("err/x.png", stream),
            Times.Once);
    }
    // --------------------------------------------------------------------
    // EDGE TEST 1
    // folder = ""  => Expect path starts with "/fileName"
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Handle_Empty_Folder()
    {
        var stream = new MemoryStream(new byte[] { 1 });
        string folder = "";
        string fileName = "img.png";

        string? capturedPath = null;

        _storage.Setup(s => s.SaveAsync(It.IsAny<string>(), stream, It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((p, s, _) => capturedPath = p)
                .Returns(Task.CompletedTask);

        var result = await _useCase.UploadAsync(stream, fileName, folder, default);

        result.Should().Be("/img.png");
        capturedPath.Should().Be("/img.png");
    }

    // --------------------------------------------------------------------
    // EDGE TEST 2
    // fileName = ""  => Expect path becomes "folder/"
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Handle_Empty_FileName()
    {
        var stream = new MemoryStream(new byte[] { 1 });
        string folder = "images";
        string fileName = "";

        string? capturedPath = null;

        _storage.Setup(s => s.SaveAsync(It.IsAny<string>(), stream, It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((p, s, _) => capturedPath = p)
                .Returns(Task.CompletedTask);

        var result = await _useCase.UploadAsync(stream, fileName, folder, default);

        result.Should().Be("images/");
        capturedPath.Should().Be("images/");
    }

    // --------------------------------------------------------------------
    // EDGE TEST 3
    // fileStream is empty (Length = 0) => Should still call SaveAsync normally
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Handle_Empty_Stream()
    {
        var emptyStream = new MemoryStream(); // no content
        string folder = "uploads";
        string fileName = "zero.bin";

        await _useCase.UploadAsync(emptyStream, fileName, folder, default);

        _storage.Verify(
            s => s.SaveAsync("uploads/zero.bin", emptyStream),
            Times.Once);
    }

    // --------------------------------------------------------------------
    // EDGE TEST 4
    // fileStream throws exception when accessed => exception must propagate
    // --------------------------------------------------------------------
    [Fact]
    public async Task Should_Throw_When_Stream_Fails()
    {
        // A stream that throws any time it's read
        var faultyStream = new FaultyStream();

        string folder = "bad";
        string fileName = "error.png";

        _storage
            .Setup(s => s.SaveAsync("bad/error.png", faultyStream))
            .ThrowsAsync(new IOException("stream crash"));

        Func<Task> act = () =>
            _useCase.UploadAsync(faultyStream, fileName, folder, default);

        await act.Should().ThrowAsync<IOException>()
                 .WithMessage("stream crash");
    }

    public class FaultyStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length => throw new Exception("fail");
        public override long Position { get => 0; set => throw new Exception("fail"); }

        public override void Flush() => throw new Exception("fail");
        public override int Read(byte[] buffer, int offset, int count)
            => throw new Exception("fail");
        public override long Seek(long offset, SeekOrigin origin)
            => throw new Exception("fail");
        public override void SetLength(long value)
            => throw new Exception("fail");
        public override void Write(byte[] buffer, int offset, int count)
            => throw new Exception("fail");
    }

}
