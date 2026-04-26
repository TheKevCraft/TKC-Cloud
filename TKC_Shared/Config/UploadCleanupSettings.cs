namespace TKC_Shared.Config;

public class UploadCleanupSettings
{
    public int IntervalMinutes { get; set; } = 10;
    public int ExpirationHours { get; set; } = 6;
}