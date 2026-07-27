using DotNetEnv;
using Microsoft.AspNetCore.Http.Features;
using TKC_Cloud.Services.Config.Models;
using TKC_Cloud.Services.Secret;

namespace TKC_Cloud.Extensions;

internal static class ConfigurationsExtensions
{
    internal static IServiceCollection AddCloudConfiguration(this IServiceCollection services, IConfigurationService config)
    {
        // Load .env
        Env.Load();

        // Configuration
        services.AddSingleton<IConfigurationService>(config);

        foreach (var (type, instance) in config.GetAll())
        {
            services.AddSingleton(type, instance);
        }

        // Secrets
        services.AddSingleton<ISecretProvider, EnvironmentSecretProvider>();

        return services;
    }

    internal static IServiceCollection AddCloudServer(this IServiceCollection services, WebApplicationBuilder builder, IConfigurationService config)
    {
        // Temporer generate ConfigurationService,

        var server = config.Get<ServerOptions>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = server.MaxUploadSize;

            options.ListenAnyIP(server.Port, listen =>
            {
                if (server.Https)
                    listen.UseHttps();
            });
        });

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = server.MaxUploadSize;
        });

        services.AddCors(options =>
        {
            options.AddPolicy("default", policy =>
            {
                policy
                    .WithOrigins(server.Cors.Origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders(server.Cors.WithExposedHeaders)
                    .SetPreflightMaxAge(
                        TimeSpan.FromHours(server.Cors.PreflightMaxAgeHours));
            });
        });

        return services;
    }
}