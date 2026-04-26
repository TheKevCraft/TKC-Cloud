namespace TKC_Cloud.Services.FileService;

public interface IFileService
{
    // Gets
    Task<List<FileEntry>> GetAllAsync(Guid userId);
    
    // Donwload
    Task<(Stream Stream, FileEntry Info)?> DownloadAsync(Guid id, Guid userId);
    
    // Upload
    Task<FileEntry> UploadAsync(IFormFile file, Guid userId);
    //Task<FileEntry> UploadStreamAsync(Stream stream, string fileName, string? contentType, Guid? folderId, Guid userId);
    Task<UploadSession> CreateUploadSessionAsync(string fileName, long totalSize, int chunkSize, string? expectedHash, Guid userId);
    Task UploadChunkAsync(Guid sessionId, int chunkIndex, Stream chunkStream, Guid userId);
    Task<object?> GetMissingChunksAsync(Guid sessionId, Guid userId);
    Task<FileEntry> FinalizeUploadAsync(Guid sessionId, Guid userId);
    Task<object?> GetUploadProgressAsync(Guid sessionId, Guid userId);

    // Delete
    Task<bool> SoftDeleteFileAsync(Guid id, Guid userId);
    Task<bool> SoftDeleteFolderAsync(Guid id, Guid userId);
}