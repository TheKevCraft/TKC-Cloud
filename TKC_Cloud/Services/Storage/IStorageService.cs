namespace TKC_Cloud.Services.Storage;

public interface IStorageService
{
    Task CreateFileAsync(Guid userId,string fileName);
    Task AppendChunkAsync(Guid userId,string fineName, long position, Stream data);
    Task<Stream> OpenReadAsync(Guid userId,string fileName);
    Task MoveAsync(Guid userId,string source, string destination);
    Task DeleteAsync(Guid userId,string fileName);

    Task<long> GetSizeAsync(Guid userId, string fileName);
    
    Task<bool> Exists(Guid userId,string fileName);
}