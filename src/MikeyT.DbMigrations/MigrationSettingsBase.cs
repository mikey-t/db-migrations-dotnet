namespace MikeyT.DbMigrations;

public abstract class MigrationSettingsBase
{
    protected Dictionary<string, string> EnvPairs = new();

    public MigrationSettingsBase Init()
    {
        var errors = PopulateAndValidate();
        if (!errors.Any()) return this;
        throw new Exception(@"Invalid env values for migrator settings: " + string.Join(", ", errors));
    }

    protected virtual List<string> PopulateAndValidate()
    {
        List<string> errors = new();

        var keys = GetEnvKeys();

        foreach (var key in keys)
        {
            var val = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(val))
            {
                errors.Add($"{key} is missing");
            }
            else
            {
                EnvPairs.Add(key, val);
            }
        }

        return errors;
    }

    protected abstract List<string> GetEnvKeys();
    public abstract string GetRootConnectionString();
    public abstract string GetMigrationsConnectionString();
    public abstract string GetTestMigrationsConnectionString();
    public abstract string GetLogSafeConnectionString(string connectionString);
}
