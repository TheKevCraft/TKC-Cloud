using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;
using TKC_Cloud.Services.Storage;

namespace TKC_Cloud.Services.Cleanup;

public class StorageCleanupService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<StorageCleanupService> _logger;

    public StorageCleanupService(IServiceProvider provider, ILogger<StorageCleanupService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _provider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var factory = scope.ServiceProvider.GetRequiredService<IStorageServiceFactory>();
        var storage = factory.Create();

        _logger.LogInformation("Starting storage cleanup...");

        var users = await db.Users.ToListAsync(stoppingToken);

        foreach (var user in users)
        {
            var storageFiles = await storage.ListFilesAsync(user.Id);
            var dbFiles = await db.Files
                .Where(f => f.OwnerId == user.Id)
                .Select(f => f.StoredFileName)
                .ToListAsync(stoppingToken);

            foreach (var file in storageFiles)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    if (!dbFiles.Contains(file))
                    {
                        _logger.LogWarning("Deleting orphan file: {}", file);
                        await storage.DeleteAsync(user.Id, file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex ,"Failed to delete orphan file {file}", file);
                }
            }
        }
    }
}