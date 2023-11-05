namespace MikeyT.DbMigrations;

public class PostgresSettings : DbSettings
{
    private readonly IDotEnv _dotEnv;
    private readonly IEnvHelper _envHelper;

    public string Host { get; }
    public string Port { get; }
    public string DbName { get; }
    public string DbUser { get; }
    public string DbPassword { get; }
    public string DbRootUser { get; }
    public string DbRootPassword { get; }

    public PostgresSettings(Type dbContextType) : this(dbContextType, new DotEnv(), new EnvHelper()) { }

    public PostgresSettings(Type dbContextType, IDotEnv dotEnv, IEnvHelper envHelper) : base(dbContextType)
    {
        _dotEnv = dotEnv;
        _envHelper = envHelper;

        _envHelper.AssignSubstitutions(EnvSubstitutions);

        _dotEnv.Load();

        Host = _envHelper.GetRequiredString("DB_HOST");
        Port = _envHelper.GetRequiredString("DB_PORT");
        DbName = _envHelper.GetRequiredString("DB_NAME");
        DbUser = _envHelper.GetRequiredString("DB_USER");
        DbPassword = _envHelper.GetRequiredString("DB_PASSWORD");
        DbRootUser = _envHelper.GetRequiredString("DB_ROOT_USER");
        DbRootPassword = _envHelper.GetRequiredString("DB_ROOT_PASSWORD");
    }

    public string GetRootConnectionString()
    {
        return GetConnectionString("postgres", DbRootUser, DbRootPassword);
    }

    public override string GetMigrationsConnectionString()
    {
        return GetConnectionString(DbName, DbRootUser, DbRootPassword);
    }

    public string GetLogSafeConnectionString(string connectionString)
    {
        return connectionString.Replace(DbPassword, "******").Replace(DbRootPassword, "******");
    }

    private string GetConnectionString(string dbName, string dbUser, string dbPassword)
    {
        var connectionString = $"Host={Host};Port={Port};Database={dbName};User Id={dbUser};Password={dbPassword};";

        // Will include error detail unless env var POSTGRES_INCLUDE_ERROR_DETAIL is explicitly set to false
        if (_envHelper.GetString("POSTGRES_INCLUDE_ERROR_DETAIL")?.ToLower() != "false")
        {
            connectionString += "Include Error Detail=true;";
        }

        return connectionString;
    }
}
