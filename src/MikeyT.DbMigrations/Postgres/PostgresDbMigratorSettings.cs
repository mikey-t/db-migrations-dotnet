using MikeyT.EnvironmentSettingsNS.Enums;

namespace MikeyT.DbMigrations.Postgres;

public class PostgresDbMigratorSettings : MigrationSettingsBase
{
    private const string POSTGRES = "postgres";

    private readonly string KEY_HOST = DbMigrationSettings.DB_HOST.ToName();
    private readonly string KEY_PORT = DbMigrationSettings.DB_PORT.ToName();
    private readonly string KEY_DB_NAME = DbMigrationSettings.DB_NAME.ToName();
    private readonly string KEY_ROOT_USER = DbMigrationSettings.DB_ROOT_USER.ToName();
    private readonly string KEY_ROOT_PASS = DbMigrationSettings.DB_ROOT_PASSWORD.ToName();
    private readonly string KEY_USER = DbMigrationSettings.DB_USER.ToName();
    private readonly string KEY_PASS = DbMigrationSettings.DB_PASSWORD.ToName();

    protected override List<string> GetEnvKeys()
    {
        return new List<string>
        {
            KEY_HOST,
            KEY_PORT,
            KEY_DB_NAME,
            KEY_ROOT_USER,
            KEY_ROOT_PASS,
            KEY_USER,
            KEY_PASS
        };
    }

    public string GetDbUser()
    {
        return EnvPairs[KEY_USER];
    }

    public string GetDbPass()
    {
        return EnvPairs[KEY_PASS];
    }

    public string GetDbName()
    {
        return EnvPairs[KEY_DB_NAME];
    }

    public string GetTestDbName()
    {
        return $"{Constants.TEST_PREFIX}{EnvPairs[KEY_DB_NAME]}";
    }

    public override string GetRootConnectionString()
    {
        return GetConnectionString(POSTGRES, EnvPairs[KEY_ROOT_USER], EnvPairs[KEY_ROOT_PASS]);
    }

    public override string GetMigrationsConnectionString()
    {
        return GetConnectionString(EnvPairs[KEY_DB_NAME], EnvPairs[KEY_ROOT_USER], EnvPairs[KEY_ROOT_PASS], true);
    }

    public override string GetTestMigrationsConnectionString()
    {
        return GetConnectionString(GetTestDbName(), EnvPairs[KEY_ROOT_USER], EnvPairs[KEY_ROOT_PASS], true);
    }

    public override string GetLogSafeConnectionString(string connectionString)
    {
        return connectionString.Replace(EnvPairs[KEY_ROOT_PASS], "********");
    }

    private string GetConnectionString(string dbName, string dbUser, string dbPassword, bool withErrorDetail = false)
    {
        var connectionString = $"Host={EnvPairs[KEY_HOST]};Port={EnvPairs[KEY_PORT]};Database={dbName};User Id={dbUser};Password={dbPassword};";

        if (withErrorDetail)
        {
            connectionString += "Include Error Detail=true;";
        }

        return connectionString;
    }
}
