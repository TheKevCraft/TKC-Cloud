namespace TKC_Cloud.Services.FileService;

public interface IFileService
{
    // Gets

    /// <summary>
    /// Retrieves all files belonging to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of all file entries owned by the user.</returns>
    Task<List<FileEntry>> GetAllAsync(Guid userId);

    /// <summary>
    /// Retrieves a paged list of files for the specified user based on the given filter and paging options.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="request">The paging and filtering parameters.</param>
    /// <returns>A paged result containing file entries.</returns>
    Task<PagedResult<FileEntry>> GetPagedAsync(Guid userId, FilePagedRequest request);
    
    // Download

    /// <summary>
    /// Downloads a file stream along with its metadata for the specified user.
    /// </summary>
    /// <param name="id">The unique identifier of the file.</param>
    /// <param name="userId">The unique identifier of the user requesting the file.</param>
    /// <returns>A tuple containing the file stream and its metadata, or null if not found or access is denied.</returns>
    Task<(Stream Stream, FileEntry Info)?> DownloadAsync(Guid id, Guid userId);
    
    // Upload

    /// <summary>
    /// Uploads a file for the specified user using a single request.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="userId">The unique identifier of the user uploading the file.</param>
    /// <returns>The created file entry.</returns>
    Task<FileEntry> UploadAsync(IFormFile file, Guid userId);

    /// <summary>
    /// Creates a new chunked upload session for a large file upload.
    /// </summary>
    /// <param name="fileName">The name of the file being uploaded.</param>
    /// <param name="totalSize">The total size of the file in bytes.</param>
    /// <param name="chunkSize">The size of each upload chunk in bytes.</param>
    /// <param name="expectedHash">Optional expected hash of the complete file for integrity validation.</param>
    /// <param name="userId">The unique identifier of the user uploading the file.</param>
    /// <returns>The created upload session information.</returns>
    Task<UploadSession> CreateUploadSessionAsync(string fileName, long totalSize, int chunkSize, string? expectedHash, Guid userId);

    /// <summary>
    /// Uploads a single chunk of a previously created upload session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the upload session.</param>
    /// <param name="chunkIndex">The index of the chunk being uploaded.</param>
    /// <param name="chunkStream">The data stream of the chunk.</param>
    /// <param name="userId">The unique identifier of the user uploading the chunk.</param>
    Task UploadChunkAsync(Guid sessionId, int chunkIndex, Stream chunkStream, Guid userId);

    /// <summary>
    /// Retrieves information about missing chunks for an ongoing upload session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the upload session.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list or object describing missing chunks, or null if the session is not found.</returns>
    Task<object?> GetMissingChunksAsync(Guid sessionId, Guid userId);

    /// <summary>
    /// Finalizes a chunked upload session and creates the final file entry.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the upload session.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The created file entry after successful upload completion.</returns>
    Task<FileEntry> FinalizeUploadAsync(Guid sessionId, Guid userId);

    /// <summary>
    /// Retrieves the current upload progress of a chunked upload session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the upload session.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>An object containing progress information, or null if the session is not found.</returns>
    Task<object?> GetUploadProgressAsync(Guid sessionId, Guid userId);

    // Delete

    /// <summary>
    /// Soft deletes a file for the specified user without permanently removing it.
    /// </summary>
    /// <param name="id">The unique identifier of the file.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>True if the file was successfully soft deleted; otherwise false.</returns>
    Task<bool> SoftDeleteFileAsync(Guid id, Guid userId);

    /// <summary>
    /// Soft deletes a folder for the specified user without permanently removing it.
    /// </summary>
    /// <param name="id">The unique identifier of the folder.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>True if the folder was successfully soft deleted; otherwise false.</returns>
    Task<bool> SoftDeleteFolderAsync(Guid id, Guid userId);
}