namespace TKC_Cloud.Services.Config;

public class AuthOptions
{
    public Registation Registation { get; set; } = new();

    public Password Password { get; set; } = new();

    public JWT_Config JWT_Config { get; set; } = new();
}
public class Registation
{
    public bool AllowRegistration { get; set; }

    public bool RequireEmailConfirmation { get; set; }
}

public class Password
{
    public int MinimumLength { get; set; }
}

public class JWT_Config
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenLifetimeHours { get; set; }

    public int RefeshTokenLifetimeDays { get; set; }   
}