namespace TKC_Cloud.Services.Config.Models;

public class ServerOptions
{
    public int Port { get; set; }
    public bool Https { get; set; }
    public long MaxUploadSize { get; set; }
    public string? PublicUrl { get; set; }

    public Cors Cors { get; set; } = new();
}

public class Cors
{
    public string[] Origins { get; set; } = [];
    
    public string[] WithExposedHeaders { get; set; } = [];

    public int PreflightMaxAgeHours { get; set; } = 1;
}