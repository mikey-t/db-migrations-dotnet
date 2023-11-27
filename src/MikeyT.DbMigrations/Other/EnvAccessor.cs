namespace MikeyT.DbMigrations;

public interface IEnvAccessor
{
    public string GetString(string key);
    public string GetRequiredString(string key);
}

public class EnvAccessor : IEnvAccessor
{
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
        var val = Environment.GetEnvironmentVariable(key);
        if (required && string.IsNullOrWhiteSpace(val))
        {
            throw new Exception("Missing environment variable for key " + key);
        }

        return val ?? string.Empty;
    }
}
