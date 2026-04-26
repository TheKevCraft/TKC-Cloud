using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TKC_Cloud.Data;
using Microsoft.EntityFrameworkCore;

namespace TKC_Cloud.Controllers;

[ApiController]
[Route("api/share")]
public class ShareController : ControllerBase
{
    private readonly AppDbContext _db;

    public ShareController(AppDbContext db)
    {
        _db = db;
    }

    // Create Share
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateShareDto dto)
    {
        var userId = GetUserId();

        var share = new Share
        {
            Id = Guid.NewGuid(),
            FileId = dto.FileId,
            OwnerId = userId,
            Mode = dto.Mode,
            Token = GenerateToken(),
            ExpireAt = dto.ExpireAt,
            MaxViews = dto.MaxViews,
            MaxDownloads = dto.MaxDownloads,
            AllowDownload = dto.AllowDownload
        };

        if (!string.IsNullOrEmpty(dto.Password))
            share.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        if (dto.UserIds?.Any() == true)
        {
            share.Permissions = dto.UserIds.Select(u => new SharePermission
            {
                Id = Guid.NewGuid(),
                UserId = u
            }).ToList();
        }

        _db.Shares.Add(share);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            share.Id,
            share.Token,
            Url = $"share/s/{share.Token}"
        });
    }

    // View Owner
    [HttpGet("{fileId}")]
    [Authorize]
    public async Task<IActionResult> GetShares(Guid fileId)
    {
        var userId = GetUserId();

        var shares = await _db.Shares
            .Where(s => s.FileId == fileId && s.OwnerId == userId)
            .ToListAsync();

        return Ok(shares);
    }

    // Edit Share
    [HttpPut("id")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateShareDto dto)
    {
        var userId = GetUserId();

        var share = await _db.Shares
            .Include(s => s.Permissions)
            .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);

        if (share == null)
            return NotFound();

        share.Mode = dto.Mode;
        share.ExpireAt = dto.ExpireAt;
        share.MaxViews = dto.MaxViews;
        share.MaxDownloads = dto.MaxDownloads;
        share.AllowDownload = dto.AllowDownload;

        // Permissions reset
        share.Permissions.Clear();

        if (dto.UserIds?.Any() == true)
        {
            share.Permissions = dto.UserIds.Select(u => new SharePermission
            {
                Id = Guid.NewGuid(),
                UserId = u
            }).ToList();
        }

        await _db.SaveChangesAsync();

        return Ok();
    }

    // Delete Share
    [HttpDelete("id")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var share = await _db.Shares
            .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);

        if (share == null)
            return NotFound();

        _db.Shares.Remove(share);
        await _db.SaveChangesAsync();

        return Ok();
    }

    // Helpers
    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException();
    
        return Guid.Parse(claim.Value);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}