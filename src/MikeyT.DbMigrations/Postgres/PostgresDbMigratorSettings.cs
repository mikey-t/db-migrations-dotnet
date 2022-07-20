using MikeyT.EnvironmentSettingsNS.Logic;

namespace MikeyT.DbMigrations.Postgres;

public class PostgresDbMigratorSettings : IDbMigratorSettings
{
    private const string TEST_DB_PREFIX = "test_";

    public string Host { get; }
    public string Port { get; }
    public string DbName { get; }
    public string TestDbName => $"{TEST_DB_PREFIX}{DbName}";
    public string DbUser { get; }
    public string DbPassword { get; }
    public string DbRootUser { get; }
    public string DbRootPassword { get; }

    public PostgresDbMigratorSettings()
    {
        DotEnv.Load();
        Host = MiscUtil.GetEnvString("DB_HOST");
        Port = MiscUtil.GetEnvString("DB_PORT");
        DbName = MiscUtil.GetEnvString("DB_NAME");
        DbUser = MiscUtil.GetEnvString("DB_USER");
        DbPassword = MiscUtil.GetEnvString("DB_PASSWORD");
        DbRootUser = MiscUtil.GetEnvString("DB_ROOT_USER");
        DbRootPassword = MiscUtil.GetEnvString("DB_ROOT_PASSWORD");
    }

    public string GetRootConnectionString()
    {
        return GetConnectionString("postgres", DbRootUser, DbRootPassword);
    }

    public string GetMigrationsConnectionString()
    {
        return GetConnectionString(DbName, DbRootUser, DbRootPassword, true);
    }

    public string GetTestMigrationsConnectionString()
    {
        return GetConnectionString(TestDbName, DbRootUser, DbRootPassword, true);
    }

    public string GetLogSafeConnectionString(string connectionString)
    {
        return connectionString.Replace(DbPassword, "******").Replace(DbRootPassword, "******");
    }

    private string GetConnectionString(string dbName, string dbUser, string dbPassword, bool withErrorDetail = false)
    {
        var connectionString = $"Host={Host};Port={Port};Database={dbName};User Id={dbUser};Password={dbPassword};";

        if (withErrorDetail)
        {
            connectionString += "Include Error Detail=true;";
        }

        return connectionString;
    }
}
