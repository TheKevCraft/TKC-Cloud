using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TKC_Cloud.Services;

namespace TKC_Cloud.Controllers;

[Authorize]
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    // Main Controller for all actions with files and Folders.
    // For all actions is an Authorition requested
    // v1.0
    private readonly IFileService _fileService;
    private readonly FileAccessTokenService _tokenService;

    public FilesController(IFileService fileService, FileAccessTokenService tokenService)
    {
        _fileService = fileService;
        _tokenService = tokenService;
    }

    // Get All
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var files = await _fileService.GetAllAsync(userId);
        return Ok(files);
    }

    #region Preview
    [HttpPost("{id}/create-access")]
    public async Task<IActionResult> CreateAccess(Guid id)
    {
        var userId = GetUserId();

        var token =  _tokenService.CreateToken(id, userId, 60);

        return Ok(new { token });
    }

    [AllowAnonymous]
    [HttpGet("preview")]
    public async Task<IActionResult> PreviewWithToken([FromQuery] string access)
    {
        var entry = _tokenService.Validate(access);

        if (entry == null)
            return Unauthorized();

        var result = await _fileService.DownloadAsync(entry.FileId, entry.UserId);

        if (result == null)
            return NotFound();

        return File(
            result.Value.Stream,
            result.Value.Info.ContentType,
            enableRangeProcessing: true
        );
    }

    [AllowAnonymous]
    [HttpGet("{id}/preview")]
    public async Task<IActionResult> Preview(Guid id)
    {
        var userId = GetUserId();
        var result = await _fileService.DownloadAsync(id, userId);

        if (result == null)
            return NotFound();

        return File(
            result.Value.Stream,
            result.Value.Info.ContentType,
            enableRangeProcessing: true
        );
    }
    #endregion

    #region Downlod
    [HttpGet("{id}")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> Download(Guid id)
    {
        var userId = GetUserId();
        var result = await _fileService.DownloadAsync(id, userId);

        if (result == null)
            return NotFound();

        return File(
            result.Value.Stream,
            result.Value.Info.ContentType,
            result.Value.Info.OriginalFileName
        );
    }
    #endregion
    
    #region Upload
    // Upload for a single smal File
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        var userId = GetUserId();
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var result = await _fileService.UploadAsync(file, userId);

        return Ok(result);
    }

    /*[HttpPost("upload-stream")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadStream(
        [FromQuery] string fileName, 
        [FromQuery] string? contentType, 
        [FromQuery] Guid? folderId)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("FileName missing.");

        var result = await _fileService.UploadStreamAsync(
            Request.Body,
            fileName,
            contentType,
            folderId,
            userId);

        return Ok(result);
    }*/

    // Create an upload session
    [HttpPost("create-upload-session")]
    public async Task<IActionResult> CreateUploadSession(
        [FromQuery] string fileName, 
        [FromQuery] long totalSize, 
        [FromQuery] int chunkSize,
        [FromQuery] string? expectedHash)
    {
        var userId = GetUserId();

        var session = await _fileService.CreateUploadSessionAsync(
            fileName,
            totalSize,
            chunkSize,
            expectedHash,
            userId);

        return Ok(session);
    }

    // upload in chunks 
    [HttpPost("upload-chunk/{sessionId}/{chunkIndex}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadChunk(Guid sessionId, int chunkIndex)
    {
        var userId = GetUserId();
        await _fileService.UploadChunkAsync(sessionId, chunkIndex, Request.Body, userId);
        return Ok();
    }

    // reupload missing chunks to repare files
    [HttpGet("missing-chunks/{sessionId}")]
    public async Task<IActionResult> GetMissingChunks(Guid sessionId)
    {
        var userId = GetUserId();

        var missing = await _fileService.GetMissingChunksAsync(sessionId, userId);

        if (missing == null)
            return NotFound();

        return Ok(missing);
    }
    
    // kombinde chunks to the final file and checks for fault`s
    [HttpPost("finalize-Upload/{sessionId}")]
    public async Task<IActionResult> FinalizeUpload(Guid sessionId)
    {
        var userId = GetUserId();
        var result = await _fileService.FinalizeUploadAsync(sessionId, userId);
        return Ok(result);
    }

    // view the upload state
    [HttpGet("upload-progress/{sessionId}")]
    public async Task<IActionResult> UploadProgress(Guid sessionId)
    {
        var userId = GetUserId();

        var progress = await _fileService.GetUploadProgressAsync(sessionId, userId);

        if (progress == null)
            return NotFound();

        return Ok(progress);
    }
    #endregion

    #region Delete
    // Delte a File
    [HttpDelete("file/{id}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        var userId = GetUserId();
        var success = await _fileService.SoftDeleteFileAsync(id, userId);
        return success ? Ok() : NotFound();
    }

    // Delete a Folder
    [HttpDelete("folder/{id}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        var userId = GetUserId();
        var success = await _fileService.SoftDeleteFolderAsync(id, userId);
        return success ? Ok() : NotFound();
    }
    #endregion

    // Helpers
    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException();
    
        return Guid.Parse(claim.Value);
    }
}