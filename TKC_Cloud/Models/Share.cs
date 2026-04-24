namespace TKC_Cloud.Models;

public class Share
{
    public Guid Id { get; set; }

    public Guid FileId { get; set; }
    public Guid OwnerId { get; set; }

    public ShareMode Mode { get; set; }

    public string Token { get; set; } = null!;
    
    public string? PasswordHash { get; set; }
    
    public DateTime ExpireAt { get; set; }
    
    public int? MaxViews { get; set; }
    public int Views { get; set; }
    
    public int? MaxDownloads { get; set; }
    public int Downloads { get; set; }
    
    public bool AllowDownload { get; set; } = true;

    public List<SharePermission> Permissions { get; set; } = new();
}