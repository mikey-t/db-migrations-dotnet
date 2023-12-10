namespace MikeyT.DbMigrations;

public class SqlServerEnvKeys
{
    public string DbHostKey { get; set; } = "DB_HOST";
    public string DbPortKey { get; set; } = "DB_PORT";
    public string DbNameKey { get; set; } = "DB_NAME";
    public string DbUserKey { get; set; } = "DB_USER";
    public string DbPasswordKey { get; set; } = "DB_PASSWORD";
    public string DbRootUserKey { get; set; } = "DB_ROOT_USER";
    public string DbRootPasswordKey { get; set; } = "DB_ROOT_PASSWORD";
}

public class SqlServerSettings : DbSettings
{
    private readonly IDotEnvLoader _dotEnvLoader;
    private readonly IEnvAccessor _envAccessor;
    private readonly SqlServerEnvKeys _envKeys;

    private string
        _dbHost = string.Empty,
        _dbPort = string.Empty,
        _dbRootUser = string.Empty,
        _dbRootPassword = string.Empty,
        _dbName = string.Empty,
        _dbUser = string.Empty,
        _dbPassword = string.Empty;


    public SqlServerSettings() : this(new SqlServerEnvKeys(), new DotEnvLoader(), new EnvAccessor()) { }

    public SqlServerSettings(SqlServerEnvKeys envKeys) : this(envKeys, new DotEnvLoader(), new EnvAccessor()) { }

    public SqlServerSettings(SqlServerEnvKeys envKeys, IDotEnvLoader dotEnvLoader, IEnvAccessor envAccessor)
    {
        _envKeys = envKeys ?? new SqlServerEnvKeys();
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
        return GetConnectionString("master", _dbRootUser, _dbRootPassword);
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

    private string GetConnectionString(string dbName, string dbUser, string dbPassword)
    {
        return $"Server={_dbHost},{_dbPort};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
    }

}
