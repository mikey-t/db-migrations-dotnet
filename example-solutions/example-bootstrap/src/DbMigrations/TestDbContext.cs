using MikeyT.DbMigrations;

namespace DbMigrations;

public class TestDbContext : PostgresMigrationsDbContext
{
    public override PostgresSetup GetDbSetup()
    {
        return new PostgresSetup(new PostgresEnvKeys { DbName = "DB_NAME_TEST" });
    }
}
