using TKC_Cloud.Services.Config.Models;

namespace TKC_Cloud.Services.Config;

internal class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configPath;
    private readonly Dictionary<Type, object> _configs = new();
    private readonly Dictionary<Type, string> _files = new();

    public ConfigurationService(IWebHostEnvironment environment, ILogger<ConfigurationService> logger)
    {
        _logger = logger;

        _configPath = Path.Combine(environment.ContentRootPath, "config");

        Load();
    }

    private void Load()
    {
        try
        {
            Register<ServerOptions>("server.toml");
            Register<StorageOptions>("storage.toml");
            Register<DatabaseOptions>("database.toml");
            Register<AuthOptions>("auth.toml");

            _logger.LogInformation("Configuration loaded successfully.");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Configuration file misssing.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to load configuration.");
            throw;
        }
    }

    private void Register<T>(string file)
        where T : class
    {
        var path = Path.Combine(_configPath, file);

        _logger.LogDebug(
            "Loading configuration {ConfigType} from {Path}",
            typeof(T).Name,
            path
        );

        var config = ConfigLoader.Load<T>(path);

        _logger.LogInformation("Loaded configuration {ConfigType}", typeof(T).Name);

        _configs[typeof(T)] = config;

        _files[typeof(T)] = path;
    }

    public T Get<T>()
        where T : class
    {
        if (_configs.TryGetValue(typeof(T), out var config))
            return (T)config;

        throw new InvalidOperationException($"Configuration '{typeof(T).Name}' not registered.");
    }

    public IReadOnlyDictionary<Type, object> GetAll()
    {
        return _configs;
    }

    public void Reload()
    {
        foreach (var item in _files)
        {
            var type = item.Key;

            var path = item.Value;

            var method = typeof(ConfigLoader)
                .GetMethod(nameof(ConfigLoader.Load))!
                .MakeGenericMethod(type);

            _configs[type] = method.Invoke(null, new object[] { path })!;
        }
    }

    public void Save<T>()
        where T : class
    {
        var type = typeof(T);

        var config = (T)_configs[type];

        ConfigLoader.Save(_files[type], config);
    }
}