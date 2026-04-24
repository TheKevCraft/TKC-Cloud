namespace TKC_Cloud.Models;

public class UploadSession
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string OrginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;

    public long TotalSize { get; set; }

    public int TotalChunks { get; set; }

    public int ChunkSize { get; set; }

    public long UploadedBytes { get; set; }

    public string? ExpectedHash { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UploadedChunk> UploadedChunks { get; set; } = new();
}