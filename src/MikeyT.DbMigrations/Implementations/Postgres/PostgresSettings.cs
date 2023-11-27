namespace MikeyT.DbMigrations;

public class PostgresEnvKeys
{
    public string DbHostKey { get; set; } = "DB_HOST";
    public string DbPortKey { get; set; } = "DB_PORT";
    public string DbNameKey { get; set; } = "DB_NAME";
    public string DbUserKey { get; set; } = "DB_USER";
    public string DbPasswordKey { get; set; } = "DB_PASSWORD";
    public string DbRootUserKey { get; set; } = "DB_ROOT_USER";
    public string DbRootPasswordKey { get; set; } = "DB_ROOT_PASSWORD";
}

public class PostgresSettings : DbSettings
{
    private readonly IDotEnvLoader _dotEnvLoader;
    private readonly IEnvAccessor _envAccessor;
    private readonly PostgresEnvKeys _envKeys;

    private string
        _dbHost = string.Empty,
        _dbPort = string.Empty,
        _dbRootUser = string.Empty,
        _dbRootPassword = string.Empty,
        _dbName = string.Empty,
        _dbUser = string.Empty,
        _dbPassword = string.Empty;


    public PostgresSettings() : this(new PostgresEnvKeys(), new DotEnvLoader(), new EnvAccessor()) { }

    public PostgresSettings(PostgresEnvKeys envKeys) : this(envKeys, new DotEnvLoader(), new EnvAccessor()) { }

    public PostgresSettings(PostgresEnvKeys envKeys, IDotEnvLoader dotEnvLoader, IEnvAccessor envAccessor)
    {
        _envKeys = envKeys ?? new PostgresEnvKeys();
        _dotEnvLoader = dotEnvLoader;
        _envAccessor = envAccessor;
    }

    public override void Load()
    {
        _dotEnvLoader.EnsureLoaded();

        _dbHost = _envAccessor.GetRequiredString(_envKeys.DbHostKey);
        _dbPort = _envAccessor.GetRequiredString(_envKeys.DbPortKey);
        _dbName = _envAccessor.GetRequiredString(_envKeys.DbNameKey);
        _dbUser = _envAccessor.GetRequiredString(_envKeys.DbUserKey);
        _dbPassword = _envAccessor.GetRequiredString(_envKeys.DbPasswordKey);
        _dbRootUser = _envAccessor.GetRequiredString(_envKeys.DbRootUserKey);
        _dbRootPassword = _envAccessor.GetRequiredString(_envKeys.DbRootPasswordKey);
    }

    protected override string GetDbSetupConnectionStringImpl()
    {
        return GetConnectionString("postgres", _dbRootUser, _dbRootPassword);
    }

    protected override string GetMigrationsConnectionStringImpl()
    {
        return GetConnectionString(_dbName, _dbRootUser, _dbRootPassword);
    }

    public string GetDbName()
    {
        return _dbName;
    }

    public string GetDbUser()
    {
        return _dbUser;
    }

    public string GetDbPassword()
    {
        return _dbPassword;
    }

    // Error detail option will be used unless environment variable POSTGRES_INCLUDE_ERROR_DETAIL is explicitly set to false
    private string GetConnectionString(string dbName, string dbUser, string dbPassword)
    {
        var connectionString = $"Host={_dbHost};Port={_dbPort};Database={dbName};User Id={dbUser};Password={dbPassword};";

        if (_envAccessor.GetString("POSTGRES_INCLUDE_ERROR_DETAIL")?.ToLower() != "false")
        {
            connectionString += "Include Error Detail=true;";
        }

        return connectionString;
    }
}
