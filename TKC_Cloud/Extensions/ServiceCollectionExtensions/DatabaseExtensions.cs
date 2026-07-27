using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Services.Config.Models;
using TKC_Cloud.Services.Initialization;

namespace TKC_Cloud.Extensions;

internal static class DatabaseExtensions
{
    internal static IServiceCollection AddCloudDatabase(this IServiceCollection services, IConfigurationService config)
    {
        var database = config.Get<DatabaseOptions>();

        switch (database.Provider)
        {
            case "SQLite":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(database.ConnectionString));
                break;

            case "PostgreSQL":
                services.AddDbContext<AppDbContext>(options => 
                    options.UseNpgsql(database.ConnectionString));
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported database provider '{database.Provider}'.");
        }

        services.AddScoped<IApplicationInitializer, DatabaseInitializer>();

        return services;
    }
}