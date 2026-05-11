using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PixelForge.Application.Interfaces;
using PixelForge.Infrastructure.Cache;
using PixelForge.Infrastructure.Caching;
using PixelForge.Infrastructure.ImageProcessing;
using PixelForge.Infrastructure.Options;
using PixelForge.Infrastructure.Storage;

namespace PixelForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();

        // Bind StorageOptions
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.Configure<WatermarkOption>(configuration.GetSection("Watermark"));
        services.Configure<ThumbnailOption>(configuration.GetSection("Thumbnail"));

        // Register LocalStorageService
        services.AddSingleton<IStorageService, LocalStorageService>();
        services.AddSingleton<IImageProcessor, MagickImageProcessor>();
        services.AddSingleton<ImageProccessingHelper>();
        //services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "PixelForge:";
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1)
            };
        });

        services.AddSingleton<ICacheService, HybridCacheService>();//It is Hybrid Caching (Memory + Redis)
        services.AddSingleton<IHybridCacheWrapper, HybridCacheWrapper>();//It is Hybrid Caching (Memory + Redis)

        return services;
    }
}
