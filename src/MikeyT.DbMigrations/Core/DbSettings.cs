namespace MikeyT.DbMigrations;

public abstract class DbSettings
{
    private readonly Type _dbContextType;
    protected readonly List<EnvSubstitution> EnvSubstitutions;

    public DbSettings(Type dbContextType)
    {
        _dbContextType = dbContextType;
        EnvSubstitutions = GetEnvSubstitutionsFromAttributes();
    }

    public abstract string GetMigrationsConnectionString();

    private List<EnvSubstitution> GetEnvSubstitutionsFromAttributes()
    {
        var envSubstitutions = new List<EnvSubstitution>();

        var attributes = _dbContextType.GetCustomAttributes(typeof(EnvSubstitutionAttribute), true);

        foreach (EnvSubstitutionAttribute attribute in attributes.Cast<EnvSubstitutionAttribute>())
        {
            envSubstitutions.Add(new EnvSubstitution(attribute.FromEnvKey, attribute.ToEnvKey));
        }

        return envSubstitutions;
    }
}
