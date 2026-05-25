using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TKC_Cloud.Services.FileService;
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
    private readonly IUserService _userService;
    private readonly FileAccessTokenService _tokenService;

    public FilesController(IFileService fileService, IUserService userService, FileAccessTokenService tokenService)
    {
        _fileService = fileService;
        _userService = userService;
        _tokenService = tokenService;
    }

    // Get All
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = _userService.GetUserId(User);
        var files = await _fileService.GetAllAsync(userId);
        return Ok(files);
    }

    // Get Files paged
    [HttpPost("paged")]
    public async Task<IActionResult> GetPaged([FromBody] FilePagedRequest request)
    {
        var userId = _userService.GetUserId(User);
        var result = await _fileService.GetPagedAsync(userId, request);

        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(5L * 1024 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null) 
        {
            return BadRequest(new 
            { 
                Error = "File is null", 
                FormKeys = Request.Form.Keys
            });
        }

        var userId = _userService.GetUserId(User);

        var uploaded = await _fileService.UploadAsync(userId, file);

        return Ok(uploaded);
    }

    #region Preview
    [HttpPost("{id}/create-access")]
    public async Task<IActionResult> CreateAccess(Guid id)
    {
        var userId = _userService.GetUserId(User);

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
        var userId = _userService.GetUserId(User);
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
        var userId = _userService.GetUserId(User);
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

    #region Delete
    // Delte a File
    [HttpDelete("file/{id}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        var userId = _userService.GetUserId(User);
        var success = await _fileService.SoftDeleteFileAsync(id, userId);
        return success ? Ok() : NotFound();
    }

    // Delete a Folder
    [HttpDelete("folder/{id}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        var userId = _userService.GetUserId(User);
        var success = await _fileService.SoftDeleteFolderAsync(id, userId);
        return success ? Ok() : NotFound();
    }
    #endregion
}