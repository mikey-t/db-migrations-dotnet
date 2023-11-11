namespace MikeyT.DbMigrations;

public interface IEnvHelper
{
    public string GetString(string key);
    public string GetRequiredString(string key);
}

public class EnvHelper : IEnvHelper
{
    private List<EnvSubstitution> _envSubstitutions = new();

    public string GetRequiredString(string key)
    {
        return GetString(key, true);
    }

    public string GetString(string key)
    {
        return GetString(key, false);
    }

    private string GetString(string key, bool required)
    {
        var keyToUse = _envSubstitutions.FirstOrDefault(s => s.FromEnvKey == key)?.ToEnvKey ?? key;

        var val = Environment.GetEnvironmentVariable(keyToUse);
        if (required && string.IsNullOrWhiteSpace(val))
        {
            throw new Exception("Missing environment variable for key " + keyToUse);
        }

        return val ?? string.Empty;
    }
}
