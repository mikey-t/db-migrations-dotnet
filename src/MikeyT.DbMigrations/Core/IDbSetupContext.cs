namespace MikeyT.DbMigrations;

public interface IDbSetupContext<T> where T : DbSetup
{
    public T GetDbSetup();
}
