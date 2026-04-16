using FluentAssertions;
using Moq;
using PixelForge.Application.DTOs;
using PixelForge.Application.Interfaces;
using PixelForge.Application.UseCases;
using PixelForge.Domain.ValueObjects;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelForge.Unit.Tests.Application
{
    public class ProcessImageUseCaseTests
    {
        private readonly Mock<IStorageService> _storage = new();
        private readonly Mock<IImageProcessor> _processor = new();
        private readonly Mock<ICacheService> _cache = new();

        private readonly ProcessImageUseCase _useCase;

        public ProcessImageUseCaseTests()
        {
            _useCase = new ProcessImageUseCase(
                _storage.Object,
                _cache.Object,
                _processor.Object
            );
        }

        private static byte[] ReadAll(Stream s)
        {
            s.Position = 0;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        private static string BuildKey(string path, TransformOptions o)
            => $"{path}:{o.Width}:{o.Height}:{o.Quality}:{o.Format}";

        // =====================================================================
        // TEST 1: Cache Hit
        // =====================================================================
        [Fact]
        public async Task Should_Return_Cached_Result_When_Exists()
        {
            var options = new TransformOptions(100, 200, 80, "jpg");
            var cacheKey = BuildKey("file.jpg", options);

            var cachedBytes = new byte[] { 1, 2, 3 };

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync(cachedBytes);

            _processor.Setup(c => c.GetImageMimeType(cachedBytes)).Returns("image/jpeg");

            var result = await _useCase.ProcessAsync(
                "file.jpg",
                options,
                CancellationToken.None);

            ReadAll(result.Stream).Should().Equal(cachedBytes);
            result.MimeType.Should().Be("image/jpeg");

            _storage.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _processor.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), options, It.IsAny<CancellationToken>()), Times.Never);
        }

        // =====================================================================
        // TEST 2: Cache Miss → Load from Storage
        // =====================================================================
        [Fact]
        public async Task Should_Load_From_Storage_When_Not_Cached()
        {
            var options = new TransformOptions(200, 300, 70, "png");
            var cacheKey = BuildKey("photo.png", options);

            _cache.Setup(c => c.GetAsync(cacheKey)).ReturnsAsync((byte[]?)null);

            var inputStream = new MemoryStream(new byte[] { 9 });
            var processed = new MemoryStream(new byte[] { 7 });

            _storage.Setup(s => s.GetAsync("photo.png", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(inputStream);

            _processor.Setup(p => p.ProcessAsync(inputStream, options, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ImageProcessResult(processed, "image/png"));

            var result = await _useCase.ProcessAsync(
                "photo.png", options, CancellationToken.None);

            ReadAll(result.Stream).Should().Equal(new byte[] { 7 });
            result.MimeType.Should().Be("image/png");

            _storage.Verify(s => s.GetAsync("photo.png", It.IsAny<CancellationToken>()), Times.Once);
        }

        // =====================================================================
        // TEST 3: Processor Should Be Called
        // =====================================================================
        [Fact]
        public async Task Should_Call_ImageProcessor()
        {
            var options = new TransformOptions(300, 400, 90, "webp");
            var cacheKey = BuildKey("x.jpg", options);

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 1, 2 });
            var processed = new MemoryStream(new byte[] { 9, 9 });

            _storage.Setup(s => s.GetAsync("x.jpg", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ImageProcessResult(processed, "image/webp"));

            var result = await _useCase.ProcessAsync(
                "x.jpg",
                options,
                CancellationToken.None);

            ReadAll(result.Stream).Should().Equal(new byte[] { 9, 9 });

            _processor.Verify(
                p => p.ProcessAsync(input, options, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        // =====================================================================
        // TEST 4: Output Stream Should Become Byte[]
        // =====================================================================
        [Fact]
        public async Task Should_Copy_Output_Stream_To_ByteArray()
        {
            var options = new TransformOptions(400, 500, 70, "webp");
            var cacheKey = BuildKey("test.jpg", options);

            _cache.Setup(c => c.GetAsync(cacheKey)).ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 3 });
            var processed = new MemoryStream(new byte[] { 10, 11, 12 });

            _storage.Setup(s => s.GetAsync("test.jpg", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ImageProcessResult(processed, "image/webp"));

            var result = await _useCase.ProcessAsync(
                "test.jpg",
                options,
                CancellationToken.None);

            ReadAll(result.Stream).Should().Equal(new byte[] { 10, 11, 12 });
        }

        // =====================================================================
        // TEST 5: Save Result To Cache
        // =====================================================================
        [Fact]
        public async Task Should_Save_To_Cache()
        {
            var options = new TransformOptions(500, 600, 60, "jpg");
            var cacheKey = BuildKey("item.jpg", options);

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 1 });
            var processed = new MemoryStream(new byte[] { 99 });

            _storage.Setup(s => s.GetAsync("item.jpg", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ImageProcessResult(processed, "image/jpeg"));

            await _useCase.ProcessAsync(
                "item.jpg",
                options,
                CancellationToken.None);

            _cache.Verify(
                c => c.SetAsync(
                    cacheKey,
                    It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 99 })),
                    TimeSpan.FromMinutes(10)
                ),
                Times.Once
            );
        }

        // =====================================================================
        // TEST 6: Storage Exception Must Propagate
        // =====================================================================
        [Fact]
        public async Task Should_Throw_When_Storage_Fails()
        {
            var options = new TransformOptions(600, 700, 80, "png");
            var cacheKey = BuildKey("fault.png", options);

            _cache.Setup(c => c.GetAsync(cacheKey)).ReturnsAsync((byte[]?)null);

            _storage.Setup(s => s.GetAsync("fault.png", It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new IOException("storage error"));

            var act = () => _useCase.ProcessAsync(
                "fault.png",
                options,
                CancellationToken.None);

            await act.Should().ThrowAsync<IOException>()
                     .WithMessage("storage error");

            _processor.Verify(
                p => p.ProcessAsync(It.IsAny<Stream>(), options, It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        // =====================================================================
        // TEST 7: Processor Exception Must Propagate
        // =====================================================================
        [Fact]
        public async Task Should_Throw_When_Processor_Fails()
        {
            var options = new TransformOptions(700, 800, 50, "jpg");
            var cacheKey = BuildKey("x.jpg", options);

            _cache.Setup(c => c.GetAsync(cacheKey)).ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 7 });

            _storage.Setup(s => s.GetAsync("x.jpg", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("processor failed"));

            var act = () => _useCase.ProcessAsync(
                "x.jpg",
                options,
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("processor failed");

            _cache.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<TimeSpan>()),
                Times.Never
            );
        }

        // =====================================================================
        // TEST 8: Cache Set Exception Must Propagate
        // =====================================================================
        [Fact]
        public async Task Should_Throw_When_Cache_Set_Fails()
        {
            var options = new TransformOptions(800, 900, 40, "png");
            var cacheKey = BuildKey("file.png", options);

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 8 });
            var processed = new MemoryStream(new byte[] { 88 });

            _storage.Setup(s => s.GetAsync("file.png", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ImageProcessResult(processed, "image/png"));

            _cache.Setup(c => c.SetAsync(cacheKey, It.IsAny<byte[]>(), It.IsAny<TimeSpan>()))
                  .ThrowsAsync(new Exception("cache save failed"));

            var act = () => _useCase.ProcessAsync(
                "file.png",
                options,
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("cache save failed");
        }
    }
}
