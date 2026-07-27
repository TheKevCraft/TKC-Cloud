namespace TKC_Cloud.Services.Config.Models;

public class StorageOptions
{
    public string Provider { get; set; } = "Local";

    public string? PreviousProvider { get; set; }

    public Local Local { get; set; } = new();

    public S3 S3 { get; set; } = new();
}

public class Local
{
    public string Path { get; set; } = "./Storage";
}

public class S3
{
    public string Endpoint { get; set; } = "";
    public string StorageBucket { get; set; } = "";
    public bool UseSSL { get; set; } = false;
    public string Region { get; set; } = "";
}