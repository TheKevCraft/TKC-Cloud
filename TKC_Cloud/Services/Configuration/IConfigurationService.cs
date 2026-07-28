namespace TKC_Cloud.Services.Config;

public interface IConfigurationService
{    
    T Get<T>() where T : class;

    bool TryGet<T>(out T? value) where T : class;

    IEnumerable<KeyValuePair<Type, object>> GetAll();

    void Reload();

    void Reload<T>() where T : class;

    void Save<T>() where T : class;

    void SaveAll();

    bool Exists<T>();

    void Dispose();
}