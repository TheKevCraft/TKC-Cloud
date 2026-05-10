namespace TKC_Shared.Config;

public class StorageSettings
{
    public string Provider { get; set; } = "Local";
    public LocalSettings Local { get; set; } = new();
    public S3Stettings S3 { get; set; } = new();
}

public class LocalSettings
{
    public string BasePath { get; set; } = "Storage";
}

public class S3Stettings
{
    public string Endpoint { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "";
    public bool UseSSL { get; set; } = false;
}