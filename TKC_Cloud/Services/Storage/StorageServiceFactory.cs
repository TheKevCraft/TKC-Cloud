using Microsoft.Extensions.Options;
using TKC_Cloud.Services.Config;
using TKC_Cloud.Services.Config.Models;

namespace TKC_Cloud.Services.Storage;

public class StorageServiceFactory : IStorageServiceFactory
{
    private readonly IServiceProvider _provider;
    private readonly StorageOptions _setting;

    public StorageServiceFactory(IServiceProvider provider, IConfigurationService config)
    {
        _provider = provider;
        _setting = config.Get<StorageOptions>()!;
    }

    public IStorageService Create()
    {
        return _setting.Provider.ToLower() switch
        {
            "local" => _provider.GetRequiredService<LocalStorageService>(),
            "minio" => _provider.GetRequiredService<MinioStorageService>(),
            _ => throw new NotSupportedException("Unknown storage provider")
        };
    }

    public IStorageService Create(string provider)
    {
        return provider.ToLower() switch
        {
            "local" => _provider.GetRequiredService<LocalStorageService>(),
            "minio" => _provider.GetRequiredService<MinioStorageService>(),
            _ => throw new NotSupportedException("Unknown storage provider")
        };
    }
}