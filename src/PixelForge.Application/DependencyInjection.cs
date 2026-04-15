using Microsoft.Extensions.DependencyInjection;
using PixelForge.Application.UseCases;

namespace PixelForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUploadImageUseCase, UploadImageUseCase>();
        services.AddScoped<IProcessImageUseCase, ProcessImageUseCase>();

        return services;
    }
}
