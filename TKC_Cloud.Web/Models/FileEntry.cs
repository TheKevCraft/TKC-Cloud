namespace TKC_Cloud.Web.Models;

public class FileEntry
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }
    
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public Guid? FolderId { get; set; }
    public Folder? Folder { get; set; }
    
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}