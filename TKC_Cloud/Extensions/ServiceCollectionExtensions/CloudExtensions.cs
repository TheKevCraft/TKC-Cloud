using TKC_Cloud.Services;
using TKC_Cloud.Services.FileService;

namespace TKC_Cloud.Extensions;

internal static class CloudExtensions
{
    internal static IServiceCollection AddCloudServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddSingleton<FileAccessTokenService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}