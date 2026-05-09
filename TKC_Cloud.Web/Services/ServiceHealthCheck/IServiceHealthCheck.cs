namespace TKC_Cloud.Web.Services.ServicesHealthCheck;

public interface IServiceHealthCheck
{
    string Name { get; }
    Task<ServiceStatus> CheckAsync();
}

public class ServiceStatus
{
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Messsage { get; set; }
    public DateTime CheckedAt { get; set; }
}