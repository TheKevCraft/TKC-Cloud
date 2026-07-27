using Tomlyn;

namespace TKC_Cloud.Services.Config;

internal static class ConfigLoader
{
    public static T Load<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        string text = File.ReadAllText(path);

        return TomlSerializer.Deserialize<T>(text)!;
    }

    public static void Save<T>(string path, T config)
    {
        File.WriteAllText(path, TomlSerializer.Serialize(config));
    }
}