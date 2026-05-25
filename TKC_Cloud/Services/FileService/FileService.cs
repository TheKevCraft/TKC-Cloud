using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;
using TKC_Cloud.Services.Storage;

namespace TKC_Cloud.Services.FileService;

public class FileService : IFileService
{
    private readonly AppDbContext _context;
    private readonly IStorageService _storage;

    public FileService(AppDbContext context, IStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    // Gets
    public async Task<List<FileEntry>> GetAllAsync(Guid userId)
    {
        return await _context.Files
            .Where(f => f.OwnerId == userId && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<PagedResult<FileEntry>> GetPagedAsync(Guid userId, FilePagedRequest request)
    {
        if (request.Skip < 0)
            request.Skip = 0;

        if (request.Take <= 0)
            request.Take = 50;

        if (request.Take >= 100)
            request.Take = 100;

        IQueryable<FileEntry> query = _context.Files
            .Where(f => f.OwnerId == userId && !f.IsDeleted);

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(f => 
                f.OriginalFileName.Contains(request.Search));
        }

        var sortDirection = request.SortDirection?.ToLowerInvariant();

        if (sortDirection == "none")
            sortDirection = null;

        // Sort
        query = (request.SortLabel?.ToLower(), sortDirection) switch
        {
            ("name", "descending") => query.OrderByDescending(f => f.OriginalFileName),
            ("name", "ascending") => query.OrderBy(f => f.OriginalFileName),

            ("size", "descending") => query.OrderByDescending(f => f.Size),
            ("size", "ascending") => query.OrderBy(f => f.Size),

            ("created", "descending") => query.OrderByDescending(f => f.CreatedAt),
            ("created", "ascending") => query.OrderBy(f => f.CreatedAt),

            _ => query.OrderByDescending(f => f.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync();

        return new PagedResult<FileEntry>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<FileEntry> UploadAsync(Guid userId, IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        var fileName = Guid.NewGuid().ToString();

        await _storage.UploadAsync(userId, fileName, stream, file.Length, file.ContentType);

        var entry = new FileEntry
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            OriginalFileName = file.FileName,
            StoredFileName = fileName,
            ContentType = file.ContentType,
            Size = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        _context.Files.Add(entry);

        await _context.SaveChangesAsync();

        return entry;
    }

    // Download

    public async Task<(Stream Stream, FileEntry Info)?> DownloadAsync(Guid id, Guid userId)
    {
        var entry = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId && !f.IsDeleted);
        if (entry == null)
            return null;

        if (!await _storage.Exists(userId, entry.StoredFileName))
            return null;

        var stream = await _storage.OpenReadAsync(userId, entry.StoredFileName);

        return (stream, entry);
    }


    // Delete
    public async Task<bool> SoftDeleteFileAsync(Guid id, Guid userId)
    {
        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);
        if (file == null) return false;

        file.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SoftDeleteFolderAsync(Guid id, Guid userId)
    {
        var folder = await _context.Folders
            .Include(f => f.Files)
            .Include(f => f.SubFolders)
            .FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);

        if (folder == null) return false;

        await MarkFolderDeletedRecursive(folder);

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task MarkFolderDeletedRecursive(Folder folder)
    {
        folder.IsDeleted = true;

        foreach (var file in folder.Files)
            file.IsDeleted = true;

        foreach (var sub in folder.SubFolders)
            await MarkFolderDeletedRecursive(sub);
    }
}