using TKC_Cloud.Services.Cleanup;
using TKC_Cloud.Services.Storage;

namespace TKC_Cloud.Extensions;

internal static class BackgroundServiceExtensions
{
    internal static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<UploadCleanupService>();
        services.AddHostedService<StorageMigrationService>();

        return services;
    }
}