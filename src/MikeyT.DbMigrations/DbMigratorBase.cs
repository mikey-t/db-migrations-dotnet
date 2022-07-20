namespace MikeyT.DbMigrations;

public abstract class DbMigratorBase
{
    public abstract Task CreateUsersAndDatabases();
    public abstract Task DropAll();
    public abstract Task DbMigrate();
}
