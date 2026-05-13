using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TKC_Cloud.Data;

namespace TKC_Cloud.Services.Storage;

public class StorageMigrationService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<StorageMigrationService> _logger;

    public StorageMigrationService(IServiceProvider provider, ILogger<StorageMigrationService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _provider.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IStorageServiceFactory>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<StorageSettings>>().Value;
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (settings.PreviousProvider == null || settings.PreviousProvider == settings.Provider)
            return;
        
        var current = factory.Create(settings.Provider);    
        var previous = factory.Create(settings.PreviousProvider);

        _logger.LogInformation("Starting storage sync from {from} to {to}",
            settings.PreviousProvider, settings.Provider);

        var files = await db.Files.ToListAsync(stoppingToken);

        foreach (var file in files)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                if (!await current.Exists(file.OwnerId, file.StoredFileName))
                {
                    using var stream = await previous.OpenReadAsync(file.OwnerId, file.StoredFileName);
                    await current.CreateFileAsync(file.OwnerId, file.StoredFileName);
                    await current.AppendChunkAsync(file.OwnerId, file.StoredFileName, 0, stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed fo {file}", file.OriginalFileName);
            }
        }

        _logger.LogInformation("Storage sync finished");
    }
}