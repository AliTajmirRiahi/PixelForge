using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using PixelForge.Application.Interfaces;
using PixelForge.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PixelForge.Unit.Tests.Infrastructure.Caching;

public class HybridCacheServiceTests
{
    private readonly Mock<IHybridCacheWrapper> _cacheMock;
    private readonly HybridCacheService _service;

    public HybridCacheServiceTests()
    {
        _cacheMock = new Mock<IHybridCacheWrapper>();
        _service = new HybridCacheService(_cacheMock.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyNotFound()
    {
        string key = "missing-key";
        CancellationToken token = CancellationToken.None;

        _cacheMock
            .Setup(c => c.GetOrCreateAsync(
                It.Is<string>(k => k == key),
                It.IsAny<Func<CancellationToken, ValueTask<byte[]?>>>(),
                token
            ))
            .Returns<string, Func<CancellationToken, ValueTask<byte[]?>>,CancellationToken>(
                async (k, factory, ct) =>
                {
                    return await factory(ct);
                });

        var result = await _service.GetAsync(key, token);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        string key = "cache-key";
        CancellationToken token = CancellationToken.None;
        byte[] value = { 1, 2, 3 };

        _cacheMock
            .Setup(c => c.GetOrCreateAsync(
                It.Is<string>(k => k == key),
                It.IsAny<Func<CancellationToken, ValueTask<byte[]?>>>(),
                token
            ))
            .ReturnsAsync(value);

        var result = await _service.GetAsync(key, token);

        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task SetAsync_StoresValue()
    {
        string key = "set-key";
        byte[] value = { 5, 6, 7 };
        TimeSpan expiration = TimeSpan.FromMinutes(1);
        CancellationToken token = CancellationToken.None;

        _cacheMock
            .Setup(c => c.SetAsync(
                key,
                value,
                expiration,
                token))
            .Returns(ValueTask.CompletedTask);

        await _service.SetAsync(key, value, expiration, token);

        _cacheMock.Verify(c => c.SetAsync(
            key,
            value,
            expiration,
            token), Times.Once);
    }


}
