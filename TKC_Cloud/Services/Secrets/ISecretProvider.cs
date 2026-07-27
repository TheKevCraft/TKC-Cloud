namespace TKC_Cloud.Services.Secret;

public interface ISecretProvider
{
    //T Get<T>() where T : ISecretDefinition, new();

    string GetRequired(string key);

    string? GetOptional(string key);

    bool Exists(string key);
}