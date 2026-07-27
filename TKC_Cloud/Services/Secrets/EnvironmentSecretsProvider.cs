using Microsoft.Extensions.Logging;

namespace TKC_Cloud.Services.Secret;

public class EnvironmentSecretProvider : ISecretProvider
{
    private readonly ILogger<EnvironmentSecretProvider> _logger;

    public EnvironmentSecretProvider(ILogger<EnvironmentSecretProvider> logger)
    {
        _logger = logger;
    }

    public string GetRequired(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogCritical(
                "Missing required secret '{SercetName}'.",
                key);

            throw new InvalidOperationException($"Missing required secret '{key}'.");
        }

        return value;
    }

    public string? GetOptional(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    public bool Exists(string key)
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key));
    }
}