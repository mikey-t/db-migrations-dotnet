namespace MikeyT.DbMigrations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EnvSubstitutionAttribute : Attribute
{
    public string FromEnvKey { get; }
    public string ToEnvKey { get; }

    public EnvSubstitutionAttribute(string fromEnvKey, string toEnvKey)
    {
        if (string.IsNullOrEmpty(fromEnvKey))
            throw new ArgumentException("fromEnvKey value cannot be null or empty", nameof(fromEnvKey));
        if (string.IsNullOrEmpty(toEnvKey))
            throw new ArgumentException("toEnvKey value cannot be null or empty", nameof(toEnvKey));

        FromEnvKey = fromEnvKey;
        ToEnvKey = toEnvKey;
    }
}
