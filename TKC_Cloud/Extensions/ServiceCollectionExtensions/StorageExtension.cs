using Minio;
using TKC_Cloud.Services.Config.Models;
using TKC_Cloud.Services.Secret;
using TKC_Cloud.Services.Storage;

namespace TKC_Cloud.Extensions;

internal static class StorageExtension
{
    internal static IServiceCollection AddStorageServices(this IServiceCollection services)
    {
        services.AddScoped<LocalStorageService>();

        services.AddScoped<MinioStorageService>();

        services.AddScoped<IStorageServiceFactory, StorageServiceFactory>();

        services.AddScoped<IStorageService>(sp =>
        {
            var factory = sp.GetRequiredService<IStorageServiceFactory>();

            return factory.Create();
        });

        services.AddSingleton<IMinioClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfigurationService>();
            var secrets = sp.GetRequiredService<ISecretProvider>();
            var storage = config.Get<StorageOptions>();

            return new MinioClient()
                .WithEndpoint(storage.S3.Endpoint)
                .WithCredentials(
                    secrets.GetRequired(SecretKeys.S3AccessKey),
                    secrets.GetRequired(SecretKeys.S3SecretKey))
                .WithSSL(storage.S3.UseSSL)
                .Build();
        });

        return services;
    }
}