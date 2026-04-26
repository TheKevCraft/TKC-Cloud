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

    // Upload
    public async Task<FileEntry> UploadAsync(IFormFile file, Guid userId)
    {
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        await _storage.CreateFileAsync(userId, storedFileName);

        await _storage.AppendChunkAsync(userId, storedFileName, 0, file.OpenReadStream());

        var entry = new FileEntry
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            Size = file.Length
        };

        _context.Files.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    /*public async Task<FileEntry> UploadStreamAsync(Stream stream, string orginalFileName, string? contentType, Guid? folderId, Guid userId)
    {
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(orginalFileName)}";

        long totalBytes = 0;

        if (folderId.HasValue)
        {
            var folderExists = await _context.Folders
                .AnyAsync(f => f.Id == folderId.Value 
                            && f.OwnerId == userId 
                            && !f.IsDeleted);
        
            if (!folderExists)
                throw new Exception("Invalid folder");
        }

        await _storage.CreateFileAsync(storedFileName);

        await _storage.AppendChunkAsync(storedFileName, 0, stream);

        var entry = new FileEntry
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            OrginalFileName = orginalFileName,
            StoredFileName = storedFileName,
            ContentType = contentType ?? "application/octet-stream",
            Size = totalBytes,
            FolderId = folderId
        };

        _context.Files.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }*/

    public async Task<UploadSession> CreateUploadSessionAsync(string fileName, long totalSize, int chunkSize, string? expectedHash, Guid userId)
    {
        var storedFileName = $"{Guid.NewGuid()}.upload";
        var totalChunks = (int)Math.Ceiling((double)totalSize / chunkSize);

        var session = new UploadSession
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            OrginalFileName = fileName,
            StoredFileName = storedFileName,
            TotalSize = totalSize,
            ChunkSize = chunkSize,
            TotalChunks = totalChunks,
            ExpectedHash = expectedHash
        };

        await _storage.CreateFileAsync(userId, session.StoredFileName);

        _context.UploadSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task UploadChunkAsync(Guid sessionId, int chunkIndex, Stream chunkStream, Guid userId)
    {
        var session = await _context.UploadSessions
            .Include(c => c.UploadedChunks)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.OwnerId == userId);

        if (session == null || session.IsCompleted)
            throw new Exception("Invalid session");

        if (chunkIndex >= session.TotalChunks)
            throw new Exception("Invalid chunk index");

        // Falls Chunk schon existiert -> ignorieren
        if (session.UploadedChunks.Any(c => c.ChunkIndex == chunkIndex))
            return;

        await _storage.AppendChunkAsync(
            userId, 
            session.StoredFileName,
            chunkIndex * session.ChunkSize,
            chunkStream);

        session.UploadedChunks.Add(new UploadedChunk
        {
            Id = Guid.NewGuid(),
            UploadSessionId = sessionId,
            ChunkIndex = chunkIndex
        });

        await _context.SaveChangesAsync();
    }

    public async Task<object?> GetMissingChunksAsync(Guid sessionId, Guid userId)
    {
        var session = await _context.UploadSessions
            .Include(s => s.UploadedChunks)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.OwnerId == userId);

        if(session == null)
            return null;

        var uploadedIndexes = session.UploadedChunks
            .Select(c => c.ChunkIndex)
            .ToHashSet();

        var missing = Enumerable.Range(0, session.TotalChunks)
            .Where(i => !uploadedIndexes.Contains(i))
            .ToHashSet();

        return new
        {
            session.TotalChunks,
            MissingChunks = missing
        };
    }

    public async Task<FileEntry> FinalizeUploadAsync(Guid sessionId, Guid userId)
    {
        var session = await _context.UploadSessions
            .Include(s => s.UploadedChunks)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.OwnerId == userId);

        if (session == null)
            throw new Exception("Session not found");

        if (session.UploadedChunks.Count != session.TotalChunks)
            throw new Exception("Not all chunks uploaded");

        using var stream = await _storage.OpenReadAsync(userId, session.StoredFileName);

        if (!string.IsNullOrWhiteSpace(session.ExpectedHash))
        {
            using var sha = SHA256.Create();

            var hash = BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", "")
                .ToLower();

            if (hash != session.ExpectedHash.ToLower())
                throw new Exception("Hash mismatch");

            stream.Position = 0;
        }

        var size = stream.Length;

        var finalFileName = $"{Guid.NewGuid()}{Path.GetExtension(session.OrginalFileName)}";

        await _storage.MoveAsync(userId, session.StoredFileName, finalFileName);

        var entry = new FileEntry
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            OriginalFileName = session.OrginalFileName,
            StoredFileName = finalFileName,
            Size = size,
            ContentType = "application/octet-stream"
        };

        _context.Files.Add(entry);

        session.IsCompleted = true;

        await _context.SaveChangesAsync();

        return entry;
    }

    public async Task<object?> GetUploadProgressAsync(Guid sessionId, Guid userId)
    {
        var session = await _context.UploadSessions
            .Include(s => s.UploadedChunks)
            .FirstOrDefaultAsync(s => s.Id == sessionId 
                                   && s.OwnerId == userId);

        if (session == null)
            return null;

        var percent = session.TotalSize == 0 ? 0 : Math.Round((double)session.UploadedBytes / session.TotalSize * 100, 2);

        return new
        {
            session.Id,
            session.OrginalFileName,
            session.UploadedBytes,
            session.TotalSize,
            Percent = percent,
            session.IsCompleted
        };
    }


    // Download
    public async Task<(Stream Stream, FileEntry Info)?> DownloadAsync(Guid id, Guid userId)
    {
        var entry = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId && !f.IsDeleted);
        if (entry == null)
            return null;

        if (!_storage.Exists(userId, entry.StoredFileName))
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