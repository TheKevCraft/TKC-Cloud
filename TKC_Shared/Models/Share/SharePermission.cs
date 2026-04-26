namespace TKC_Shared.Models.Share;

public class SharePermission
{
    public Guid Id { get; set; }

    public Guid ShareId { get; set; }
    public Share Share { get; set; } = null!;

    public Guid UserId { get; set; }
    
    public bool CanView { get; set; } = true;
    public bool CanDownload { get; set; } = true;
}