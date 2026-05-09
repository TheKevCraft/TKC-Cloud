namespace TKC_Shared.DTOs;

public class ApiHealthInfo
{
    public string Status { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Environment { get; set; } = "";
    public DateTime Timestamp { get; set; }
}