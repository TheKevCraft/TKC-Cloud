namespace TKC_Shared.Models.Token;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = "";
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    public User? User { get; set; }
}