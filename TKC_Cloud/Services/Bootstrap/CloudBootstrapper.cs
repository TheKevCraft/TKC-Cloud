using Microsoft.Extensions.Logging.Abstractions;

namespace TKC_Cloud.Services.Bootstrap;

internal static class CloudBootstrapper
{
    internal static ConfigurationService CreateConfiguration(IWebHostEnvironment environment)
    {
        return new ConfigurationService(
            environment,
            NullLogger<ConfigurationService>.Instance);
    }
}