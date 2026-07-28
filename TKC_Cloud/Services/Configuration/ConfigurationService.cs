using System.Reflection;
using System.Security.Cryptography;
using TKC_Cloud.Services.Config.Models;

namespace TKC_Cloud.Services.Config;

internal class ConfigurationService : IConfigurationService, IDisposable
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configPath;
    private readonly Dictionary<Type, ConfigEntry> _configs = new();
    private readonly ReaderWriterLockSlim _lock = new();
    //public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
    private static readonly IReadOnlyList<ConfigurationDefinition> Configurations = 
    [
        new(typeof(ServerOptions), "server.toml"),
        new(typeof(StorageOptions), "storage.toml"),
        new(typeof(DatabaseOptions), "database.toml"),
        new(typeof(AuthOptions), "auth.toml")
    ];
    
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
            foreach (var (type, file) in Configurations)
            {
                Register(type, file);

                _logger.LogInformation(
                    "Loaded {Type} from {File}",
                    type.Name,
                    file);
            }

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

    #region Register

    private void Register(Type type, string file)
    {
        RegisterMethod
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { file });
    }

    private void RegisterGeneric<T>(string file)
        where T : class
    {
        _lock.EnterWriteLock();
        try
        {
            if (_configs.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException("Configuration is already registered!");
            }

            var path = Path.Combine(_configPath, file);

            _logger.LogDebug(
                "Loading configuration {ConfigType} from {Path}",
                typeof(T).Name,
                path
            );

            using var stream = File.OpenRead(path);

            var hash = Convert.ToHexString(SHA256.HashData(stream));

            var config = ConfigLoader.Load<T>(path);

            _logger.LogInformation("Loaded configuration {ConfigType}", typeof(T).Name);

            _configs[typeof(T)] = new ConfigEntry(
                config,
                path,
                hash,
                DateTime.UtcNow
            );
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    #endregion

    #region Get`s

    public T Get<T>()
        where T : class
    {
        _lock.EnterReadLock();
        try
        {
            if (_configs.TryGetValue(typeof(T), out var config))
                return (T)config.Value;
                
            throw new InvalidOperationException($"Configuration '{typeof(T).Name}' not registered.");
        }
        finally
        {
            _lock.ExitReadLock();   
        }
    }

    public bool TryGet<T>(out T? value)
        where T : class
    {
        _lock.EnterReadLock();
        try
        {
            if (_configs.TryGetValue(typeof(T), out var obj))
            {
                value = (T)obj.Value;
                return true;
            }

            value = null;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns a snapshot of the configurations.</returns>
    public IEnumerable<KeyValuePair<Type, object>> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return _configs.ToDictionary(
                x => x.Key,
                x => x.Value.Value
            );
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public ConfigurationInfo GetInfo<T>()
    {
        _lock.EnterReadLock();
        try
        {
            if (_configs.TryGetValue(typeof(T), out var config))
                return new ConfigurationInfo
                (
                    config.File, 
                    config.Hash, 
                    config.LoadedAt
                );
                
            throw new InvalidOperationException($"Configuration '{typeof(T).Name}' not registered.");
        }
        finally
        {
            _lock.ExitReadLock();   
        }
    }

    #endregion

    #region Reload

    public void Reload()
    {
        foreach (var type in _configs.Keys.ToList())
        {
            ReloadMethod
                .MakeGenericMethod(type)
                .Invoke(this, null);
        }
    }

    public void Reload<T>()
        where T : class
    {
        _lock.EnterWriteLock();
        try
        {
            var type = typeof(T);

            if (!_configs.TryGetValue(type, out var entry))
                throw new InvalidOperationException(
                    $"Configuration '{type.Name}' not registered.");

            var config = ConfigLoader.Load<T>(entry.File);

            _configs[type] = new ConfigEntry(
                config,
                entry.File,
                null,
                DateTime.UtcNow
            );

            _logger.LogInformation(
                "Reloaded configuration {Type}",
                typeof(T).Name);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    #endregion

    #region Save

    public void Save<T>()
        where T : class
    {
        _lock.EnterReadLock();
        try
        {
            var type = typeof(T);

            var config = (T)_configs[type].Value;

            ConfigLoader.Save(_configs[type].File, config);

            using var stream = File.OpenRead(_configs[type].File);

            //_configs[type].Hash = Convert.ToHexString(SHA256.HashData(stream));

            _logger.LogInformation(
                "Saved configuration {Type}",
                typeof(T).Name);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void SaveAll()
    {
        foreach(var type in _configs.Keys)
        {
            SaveMethod
                .MakeGenericMethod(type)
                .Invoke(this, null);
        }
    }

    #endregion

    public bool Exists<T>()
    {
        _lock.EnterReadLock();
        try
        {
            return _configs.ContainsKey(typeof(T));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }

    #region MethodInfo`s

    private static readonly MethodInfo RegisterMethod =
        typeof(ConfigurationService)
            .GetMethod(nameof(RegisterGeneric), 
                BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ReloadMethod =
        typeof(ConfigurationService)
            .GetMethod(nameof(Reload), 1, Type.EmptyTypes)!;

    private static readonly MethodInfo SaveMethod =
        typeof(ConfigurationService)
            .GetMethod(nameof(Save))!;

    #endregion

    // Models
    private sealed record ConfigEntry
    (
        object Value,
        string File,
        string? Hash,
        DateTime LoadedAt
    );

    internal sealed record ConfigurationInfo
    (
        string File,
        string? Hash,
        DateTime LoadedAt
    );

    private sealed record ConfigurationDefinition
    (
        Type Type,
        string File
    );
}