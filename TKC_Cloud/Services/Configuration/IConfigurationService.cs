namespace TKC_Cloud.Services.Config;

public interface IConfigurationService
{
    
    T Get<T>() where T : class;

    IReadOnlyDictionary<Type, object> GetAll();

    void Reload();

    void Save<T>() where T : class;
}