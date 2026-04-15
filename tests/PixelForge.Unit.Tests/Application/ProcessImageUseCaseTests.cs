using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PixelForge.Application.Interfaces;
using PixelForge.Application.UseCases;
using PixelForge.Domain.ValueObjects;

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
                _processor.Object,
                _cache.Object
            );
        }


        // TEST CASE: Should return the cached bytes if the processed output already exists in cache.
        // EXPECTATION:
        //   - Cache.GetAsync must be called once.
        //   - Storage.GetAsync and Processor.ProcessAsync must NOT be called.
        [Fact]
        public async Task Should_Return_Cached_Output_When_Exists()
        {
            var options = new TransformOptions(100, 80, "jpg");
            var cacheKey = "image.jpg:100:80:jpg";

            var expected = new byte[] { 1, 2 };

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync(expected);

            var result = await _useCase.ProcessAsync("image.jpg", options);

            result.Should().Equal(expected);

            _storage.Verify(s => s.GetAsync(It.IsAny<string>()), Times.Never);
            _processor.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), options), Times.Never);
        }


        // TEST CASE: Should load the image from storage when not found in cache.
        // EXPECTATION:
        //   - Cache.GetAsync returns null.
        //   - Storage.GetAsync is called once.
        [Fact]
        public async Task Should_Load_Input_From_Storage_When_Not_In_Cache()
        {
            var options = new TransformOptions(200, 80, "webp");
            var cacheKey = "photo.png:200:80:webp";

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 9 });
            var output = new MemoryStream(new byte[] { 7 });

            _storage.Setup(s => s.GetAsync("photo.png"))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options))
                      .ReturnsAsync(output);

            var result = await _useCase.ProcessAsync("photo.png", options);

            result.Should().Equal(new byte[] { 7 });

            _storage.Verify(s => s.GetAsync("photo.png"), Times.Once);
        }


        // TEST CASE: Should call the image processor with the input stream and options.
        // EXPECTATION:
        //   - Processor.ProcessAsync should be executed exactly once.
        [Fact]
        public async Task Should_Process_Image_Using_Processor()
        {
            var options = new TransformOptions(300, 90, "png");
            var cacheKey = "x.jpg:300:90:png";

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 1, 2 });
            var output = new MemoryStream(new byte[] { 9, 9 });

            _storage.Setup(s => s.GetAsync("x.jpg"))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options))
                      .ReturnsAsync(output);

            var result = await _useCase.ProcessAsync("x.jpg", options);

            result.Should().Equal(new byte[] { 9, 9 });

            _processor.Verify(p => p.ProcessAsync(input, options), Times.Once);
        }


        // TEST CASE: Should convert processed Stream to byte[] correctly.
        // EXPECTATION:
        //   - Returned byte array must match the content of the processed output stream.
        [Fact]
        public async Task Should_Copy_Output_Stream_To_ByteArray()
        {
            var options = new TransformOptions(400, 70, "webp");
            var cacheKey = "test.jpg:400:70:webp";

            _cache.Setup(c => c.GetAsync(cacheKey)).ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 3 });
            var processed = new MemoryStream(new byte[] { 10, 11, 12 });

            _storage.Setup(s => s.GetAsync("test.jpg")).ReturnsAsync(input);
            _processor.Setup(p => p.ProcessAsync(input, options)).ReturnsAsync(processed);

            var result = await _useCase.ProcessAsync("test.jpg", options);

            result.Should().Equal(new byte[] { 10, 11, 12 });
        }


        // TEST CASE: Should save processed result into cache after successful processing.
        // EXPECTATION:
        //   - Cache.SetAsync should be called once with the correct key and bytes.
        [Fact]
        public async Task Should_Save_Result_To_Cache()
        {
            var options = new TransformOptions(500, 60, "avif");
            var cacheKey = "item.avif:500:60:avif";

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 1 });
            var output = new MemoryStream(new byte[] { 99 });

            _storage.Setup(s => s.GetAsync("item.avif")).ReturnsAsync(input);
            _processor.Setup(p => p.ProcessAsync(input, options)).ReturnsAsync(output);

            await _useCase.ProcessAsync("item.avif", options);

            _cache.Verify(
                c => c.SetAsync(cacheKey, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 99 })), TimeSpan.FromMinutes(10)),
                Times.Once
            );
        }


        // TEST CASE: Should propagate any exception thrown by storage.
        // EXPECTATION:
        //   - No processing or caching should occur.
        [Fact]
        public async Task Should_Throw_When_Storage_Fails()
        {
            var options = new TransformOptions(600, 80, "jpg");
            var cacheKey = "fault.jpg:600:80:jpg";

            _cache.Setup(c => c.GetAsync(cacheKey)).ReturnsAsync((byte[]?)null);

            _storage.Setup(s => s.GetAsync("fault.jpg"))
                    .ThrowsAsync(new IOException("storage error"));

            var act = () => _useCase.ProcessAsync("fault.jpg", options);

            await act.Should().ThrowAsync<IOException>()
                     .WithMessage("storage error");

            _processor.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), options), Times.Never);
        }


        // TEST CASE: Should propagate any exception thrown by the processor.
        // EXPECTATION:
        //   - Cache.SetAsync must NOT be called.
        [Fact]
        public async Task Should_Throw_When_Processor_Fails()
        {
            var options = new TransformOptions(700, 50, "jpg");
            var cacheKey = "x:700:50:jpg";

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 7 });

            _storage.Setup(s => s.GetAsync("x"))
                    .ReturnsAsync(input);

            _processor.Setup(p => p.ProcessAsync(input, options))
                      .ThrowsAsync(new Exception("processor failed"));

            var act = () => _useCase.ProcessAsync("x", options);

            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("processor failed");

            _cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<TimeSpan>()), Times.Never);
        }


        // TEST CASE: Should propagate any exception thrown by cache.SetAsync.
        // EXPECTATION:
        //   - Returned exception should not be swallowed.
        [Fact]
        public async Task Should_Throw_When_Cache_Set_Fails()
        {
            var options = new TransformOptions(800, 40, "png");
            var cacheKey = "f.png:800:40:png";

            _cache.Setup(c => c.GetAsync(cacheKey))
                  .ReturnsAsync((byte[]?)null);

            var input = new MemoryStream(new byte[] { 8 });
            var output = new MemoryStream(new byte[] { 88 });

            _storage.Setup(s => s.GetAsync("f.png")).ReturnsAsync(input);
            _processor.Setup(p => p.ProcessAsync(input, options)).ReturnsAsync(output);

            _cache.Setup(c => c.SetAsync(cacheKey, It.IsAny<byte[]>(), It.IsAny<TimeSpan>()))
                  .ThrowsAsync(new Exception("cache save failed"));

            var act = () => _useCase.ProcessAsync("f.png", options);

            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("cache save failed");
        }
    }
}
