namespace TKC_Cloud.Models;

public class UploadedChunk
{
    public Guid Id { get; set; }

    public Guid UploadSessionId { get; set; }
    public UploadSession UploadSession { get; set; } = null!;

    public int ChunkIndex { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}