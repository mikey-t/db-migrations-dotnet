namespace MikeyT.DbMigrations;

public class PostgresEnvKeys
{
    public string? DbHost { get; set; }
    public string? DbPort { get; set; }
    public string? DbName { get; set; }
    public string? DbUser { get; set; }
    public string? DbPassword { get; set; }
    public string? DbRootUser { get; set; }
    public string? DbRootPassword { get; set; }
}

public class PostgresSettings : DbSettings
{
    private readonly IDotEnv _dotEnv;
    private readonly IEnvHelper _envHelper;
    private readonly PostgresEnvKeys _envKeys;

    private string _dbHost = string.Empty;
    private string _dbPort = string.Empty;
    private string _dbRootUser = string.Empty;
    private string _dbRootPassword = string.Empty;

    // Fields for DbSetup (see Getter methods below)
    private string _dbName = string.Empty;
    private string _dbUser = string.Empty;
    private string _dbPassword = string.Empty;

    public PostgresSettings() : this(new PostgresEnvKeys(), new DotEnv(), new EnvHelper()) { }

    public PostgresSettings(PostgresEnvKeys envKeys) : this(envKeys, new DotEnv(), new EnvHelper()) { }

    public PostgresSettings(PostgresEnvKeys envKeys, IDotEnv dotEnv, IEnvHelper envHelper) : base()
    {
        _envKeys = envKeys;
        _dotEnv = dotEnv;
        _envHelper = envHelper;
    }

    public override void Load()
    {
        _dotEnv.Load();

        var defaultEnvKeys = new PostgresEnvKeys
        {
            DbHost = "DB_HOST",
            DbPort = "DB_PORT",
            DbName = "DB_NAME",
            DbUser = "DB_USER",
            DbPassword = "DB_PASSWORD",
            DbRootUser = "DB_ROOT_USER",
            DbRootPassword = "DB_ROOT_PASSWORD"
        };

        var mergedEnvKeys = new PostgresEnvKeys
        {
            DbHost = _envKeys.DbHost ?? defaultEnvKeys.DbHost,
            DbPort = _envKeys.DbPort ?? defaultEnvKeys.DbPort,
            DbName = _envKeys.DbName ?? defaultEnvKeys.DbName,
            DbUser = _envKeys.DbUser ?? defaultEnvKeys.DbUser,
            DbPassword = _envKeys.DbPassword ?? defaultEnvKeys.DbPassword,
            DbRootUser = _envKeys.DbRootUser ?? defaultEnvKeys.DbRootUser,
            DbRootPassword = _envKeys.DbRootPassword ?? defaultEnvKeys.DbRootPassword
        };

        _dbHost = _envHelper.GetRequiredString(mergedEnvKeys.DbHost);
        _dbPort = _envHelper.GetRequiredString(mergedEnvKeys.DbPort);
        _dbName = _envHelper.GetRequiredString(mergedEnvKeys.DbName);
        _dbUser = _envHelper.GetRequiredString(mergedEnvKeys.DbUser);
        _dbPassword = _envHelper.GetRequiredString(mergedEnvKeys.DbPassword);
        _dbRootUser = _envHelper.GetRequiredString(mergedEnvKeys.DbRootUser);
        _dbRootPassword = _envHelper.GetRequiredString(mergedEnvKeys.DbRootPassword);
    }

    public override string GetDbSetupConnectionString()
    {
        return GetConnectionString("postgres", _dbRootUser, _dbRootPassword);
    }

    public override string GetMigrationsConnectionString()
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
        var connectionString = $"Host={_dbHost};Port={_dbPort};Database={dbName};User Id={dbUser};Password={dbPassword};";

        // Error detail will be included unless environment variable POSTGRES_INCLUDE_ERROR_DETAIL is explicitly set to false
        if (_envHelper.GetString("POSTGRES_INCLUDE_ERROR_DETAIL")?.ToLower() != "false")
        {
            connectionString += "Include Error Detail=true;";
        }

        return connectionString;
    }
}
