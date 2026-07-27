namespace TKC_Cloud.Services.Config.Models;

public class DatabaseOptions
{
    public string Provider { get; set;} = "SQLite";

    public string ConnectionString { get; set; } = string.Empty;
}