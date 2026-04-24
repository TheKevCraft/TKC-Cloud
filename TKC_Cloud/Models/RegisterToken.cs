namespace TKC_Cloud.Models;

public class RegisterToken
{
    public Guid Id { get; set; }
    public string Token { get; set;} = Guid.NewGuid().ToString();
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}