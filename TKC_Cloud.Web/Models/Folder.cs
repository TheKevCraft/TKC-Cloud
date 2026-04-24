namespace TKC_Cloud.Web.Models;

public class Folder
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
    public Folder? Parent { get; set; }

    public List<Folder> SubFolders { get; set; } = new();
    public List<FileEntry> Files { get; set; } = new();

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}