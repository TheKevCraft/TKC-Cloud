using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace TKC_Cloud.Services.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly StorageSettings _settings;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IOptions<StorageSettings> options, ILogger<MinioStorageService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        _client = new MinioClient()
            .WithEndpoint(_settings.S3.Endpoint)
            .WithCredentials(_settings.S3.AccessKey, _settings.S3.SecretKey)
            .Build();
    }

    public async Task CreateFileAsync(Guid userId, string fileName)
    {
        var objectName = GetObjectName(userId, fileName);

        var stream = new MemoryStream(Array.Empty<byte>());

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.S3.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(0));

        _logger.LogInformation("Created file {File}", objectName);
    }

    public async Task AppendChunkAsync(Guid userId, string fileName, long position, Stream data)
    {
        // S3/MinIO unterstützt kein echtes "Append"
        // -> wir müssen Re-Upload oder Multipart Upload machen

        var objectName = GetObjectName(userId, fileName);

        using var memory = new MemoryStream();
        await data.CopyToAsync(memory);

        memory.Position = 0;

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.S3.BucketName)
            .WithObject(objectName)
            .WithStreamData(memory)
            .WithObjectSize(memory.Length));

        _logger.LogInformation("Uploaded chunk overwrite for {File}", objectName);
    }

    public async Task<Stream> OpenReadAsync(Guid userId, string fileName)
    {
        var ms = new MemoryStream();

        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_settings.S3.BucketName)
            .WithObject(GetObjectName(userId, fileName))
            .WithCallbackStream(stream => stream.CopyTo(ms)));

        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(Guid userId, string fileName)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_settings.S3.BucketName)
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
            .WithBucket(_settings.S3.BucketName)
            .WithObject(GetObjectName(userId, fileName)));

        return stat.Size;
    }

    public async Task<bool> Exists(Guid userId, string fileName)
    {
        try
        {
            await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_settings.S3.BucketName)
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