namespace MikeyT.DbMigrations;

public interface IDbMigratorSettings
{
    string GetMigrationsConnectionString();
    string GetTestMigrationsConnectionString();
}
