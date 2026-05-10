using Microsoft.Extensions.Options;

namespace TKC_Cloud.Services.Storage;

public class StorageServiceFactory : IStorageServiceFactory
{
    private readonly IServiceProvider _provider;
    private readonly IOptions<StorageSettings> _options;

    public StorageServiceFactory(IServiceProvider provider, IOptions<StorageSettings> options)
    {
        _provider = provider;
        _options = options;
    }

    public IStorageService Create()
    {
        return _options.Value.Provider.ToLower() switch
        {
            "local" => _provider.GetRequiredService<LocalStorageService>(),
            "minio" => _provider.GetRequiredService<MinioStorageService>(),
            _ => throw new NotSupportedException("Unknown storage provider")
        };
    }
}