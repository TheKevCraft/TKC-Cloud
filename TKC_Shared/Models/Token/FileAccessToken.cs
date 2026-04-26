namespace TKC_Shared.Models.Token;

public class FileAccessToken
{
    public string Token { get; set; } = Guid.NewGuid().ToString();
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}