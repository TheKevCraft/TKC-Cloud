using Microsoft.Extensions.Options;

namespace TKC_Cloud.Services.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IOptions<StorageSettings> options, ILogger<LocalStorageService> logger)
    {
        _basePath = Path.GetFullPath(options.Value.Local.BasePath);
        _logger = logger;

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Storage base path created at {Path}", _basePath);
        }
    }

    public async Task CreateFileAsync(Guid userId, string fileName)
    {
        var path = GetPath(userId, fileName);
        
        await using var fs = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            1,
            true);

        _logger.LogInformation("Created new file {File} for user {UserId}", fileName, userId);

        await Task.CompletedTask;
    }

    public async Task AppendChunkAsync(Guid userId, string fileName, long position, Stream data)
    {
        var path = GetPath(userId, fileName);

        try
        {
            using var fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Write,
                FileShare.None,
                81920,
                true);

            fileStream.Seek(position, SeekOrigin.Begin);
            await data.CopyToAsync(fileStream);

            _logger.LogInformation("Appended chunk at {Position} for file {File} (user {UserId})", position, fileName, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append chunk to file {File} for user {UserId}", fileName, userId);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(Guid userId, string fileName)
    {
        var path = GetPath(userId, fileName);
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(Guid userId, string fileName)
    {
        var path = GetPath(userId, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            _logger.LogInformation("Delete file {File} for user {UserId}", fileName, userId);
        }

        return Task.CompletedTask;
    }

    public Task MoveAsync(Guid userId, string source, string destination)
    {
        var src = GetPath(userId, source);
        var dest = GetPath(userId, destination);

        if (File.Exists(dest))
            File.Delete(dest);

        File.Move(src, dest);
        _logger.LogInformation("Moved file {Source} to {Dest} for user {UserId}", source, destination, userId);

        return Task.CompletedTask;
    }

    public Task<long> GetSizeAsync(Guid userId, string fileName)
    {
        var path = GetPath(userId, fileName);

        if (!File.Exists(path))
            return Task.FromResult(0L);

        var info = new FileInfo(path);
        return Task.FromResult(info.Length);
    }

    public Task<DateTime> GetCreatedAtAsync(Guid userId, string fileName)
    {
        var path = GetPath(userId, fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException();

        return Task.FromResult(File.GetCreationTimeUtc(path));
    }

    public async Task<IEnumerable<string>> ListFilesAsync(Guid userid)
    {
        var userPath = GetUserFolder(userid);

        if (!Directory.Exists(userPath))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(userPath)
            .Select(Path.GetFileName)!;
    }

    public async Task<bool> Exists(Guid userId, string fileName)
    {
        return File.Exists(GetPath(userId, fileName));
    }


    // PRIVATES
    private string GetPath(Guid userId, string fileName)
    {
        var folder = GetUserFolder(userId);

        var path = Path.Combine(folder, fileName);
        var fullPath = Path.GetFullPath(path);

        if (!fullPath.StartsWith(folder))
        {
            _logger.LogWarning("Unauthorized file path access attempt: {Path}", fullPath);
            throw new UnauthorizedAccessException("Invalid path");
        }

        return fullPath;
    }

    private string GetUserFolder(Guid userId)
    {
        var folder = Path.Combine(_basePath, userId.ToString());

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return folder;
    }
}