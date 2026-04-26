using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TKC_Cloud.Data;
using TKC_Cloud.Services.Storage;

namespace TKC_Cloud.Services.Cleanup;

public class UploadCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly UploadCleanupSettings _settings;
    private readonly ILogger<UploadCleanupService> _logger;

    public UploadCleanupService(IServiceScopeFactory scopeFactory, IOptions<UploadCleanupSettings> options, ILogger<UploadCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync();
            await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();

        var expirationTime = DateTime.UtcNow.AddHours(-_settings.ExpirationHours);

        var expiredSessions = await context.UploadSessions
            .Where(s => !s.IsCompleted && s.CreatedAt < expirationTime)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            _logger.LogInformation("Cleaning expried upload session {SessionId}", session.Id);

            await storage.DeleteAsync(session.OwnerId, session.StoredFileName);

            context.UploadSessions.Remove(session);
        }

        await context.SaveChangesAsync();
    }
}