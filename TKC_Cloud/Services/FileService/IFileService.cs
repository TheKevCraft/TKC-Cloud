namespace TKC_Cloud.Services.FileService;

public interface IFileService
{
    #region Gets

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

    #endregion

    Task<FileEntry> UploadAsync(Guid userId, IFormFile file);
    
    #region Download

    /// <summary>
    /// Downloads a file stream along with its metadata for the specified user.
    /// </summary>
    /// <param name="id">The unique identifier of the file.</param>
    /// <param name="userId">The unique identifier of the user requesting the file.</param>
    /// <returns>A tuple containing the file stream and its metadata, or null if not found or access is denied.</returns>
    Task<(Stream Stream, FileEntry Info)?> DownloadAsync(Guid id, Guid userId);

    #endregion

    #region  Delete

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

    #endregion
}