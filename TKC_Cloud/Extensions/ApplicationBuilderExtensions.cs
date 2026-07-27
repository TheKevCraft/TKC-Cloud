using TKC_Cloud.Services.Initialization;

namespace TKC_Cloud.Extensions;

internal static class ApplicationBuilderExtensions
{
    internal static async Task InitializeApplicationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializers = scope.ServiceProvider.GetServices<IApplicationInitializer>();

        foreach (var initializer in initializers)
        {
            await initializer.InitializeAsync();
        }
    }
}