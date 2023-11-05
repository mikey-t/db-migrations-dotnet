namespace MikeyT.DbMigrations;

public abstract class DbSetup
{
    public abstract Task Setup();
    public abstract Task Teardown();
}
