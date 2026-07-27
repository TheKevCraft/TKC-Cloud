using Microsoft.Extensions.Options;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using TKC_Cloud.Services.Config;
using TKC_Cloud.Services.Config.Models;

namespace TKC_Cloud.Services.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly StorageOptions _settings;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IMinioClient client, IConfigurationService config, ILogger<MinioStorageService> logger)
    {
        _settings = config.Get<StorageOptions>()!;
        _logger = logger;
        _client = client;
    }

    public async Task CreateFileAsync(Guid userId, string fileName)
    {
        var objectName = GetObjectName(userId, fileName);

        var stream = new MemoryStream(Array.Empty<byte>());

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(0));

        _logger.LogInformation("Created file {File}", objectName);
    }

    public async Task UploadAsync(Guid userId, string fileName, Stream stream, long size, string contentType)
    {
        var objectName = GetObjectName(userId, fileName);

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(size)
            .WithContentType(contentType));
    }

    public async Task AppendChunkAsync(Guid userId, string fileName, long position, Stream data)
    {
        // S3/MinIO unterstützt kein echtes "Append"
        // -> wir müssen Re-Upload oder Multipart Upload machen

        /*var objectName = GetObjectName(userId, fileName);

        using var memory = new MemoryStream();
        await data.CopyToAsync(memory);

        memory.Position = 0;

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(objectName)
            .WithStreamData(memory)
            .WithObjectSize(memory.Length));

        _logger.LogInformation("Uploaded chunk overwrite for {File}", objectName);*/

        throw new NotSupportedException();
    }

    public async Task<Stream> OpenReadAsync(Guid userId, string fileName)
    {
        var ms = new MemoryStream();

        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(GetObjectName(userId, fileName))
            .WithCallbackStream(stream => stream.CopyTo(ms)));

        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(Guid userId, string fileName)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(GetObjectName(userId, fileName)));
    }

    public Task MoveAsync(Guid userId, string source, string destination)
    {
        // S3: Copy + Delete
        throw new NotImplementedException("Move needs CopyObject + RemoveObject");
    }

    public async Task<long> GetSizeAsync(Guid userId, string fileName)
    {
        var stat = await _client.StatObjectAsync(new StatObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(GetObjectName(userId, fileName)));

        return stat.Size;
    }

    public async Task<DateTime> GetCreatedAtAsync(Guid userId, string fileName)
    {
        var key = $"{userId}/{fileName}";

        var stat = await _client.StatObjectAsync(new StatObjectArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithObject(key));

        return stat.LastModified;
    }

    public async Task<IEnumerable<string>> ListFilesAsync(Guid userId)
    {
        var prefix = $"{userId}/";

        var files = new List<string>();

        var objects = _client.ListObjectsEnumAsync(new ListObjectsArgs()
            .WithBucket(_settings.S3.StorageBucket)
            .WithPrefix(prefix)
            .WithRecursive(true));

        await foreach (var obj in objects)
        {
            files.Add(Path.GetFileName(obj.Key));
        }

        return files;
    }

    public async Task<bool> Exists(Guid userId, string fileName)
    {
        try
        {
            await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_settings.S3.StorageBucket)
                .WithObject(GetObjectName(userId, fileName)));

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetObjectName(Guid userId, string fileName)
        => $"{userId}/{fileName}";
}