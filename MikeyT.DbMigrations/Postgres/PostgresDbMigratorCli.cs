using Microsoft.EntityFrameworkCore;
using MikeyT.EnvironmentSettingsNS.Logic;

namespace MikeyT.DbMigrations.Postgres;

public class PostgresDbMigratorCli
{
    public async Task<int> Run(string[] args, DbContext mainDbContext)
    {
        try
        {
            DotEnv.Load();
            var settings = (PostgresDbMigratorSettings)new PostgresDbMigratorSettings().Init();

            if (args.Length == 0)
            {
                throw new ApplicationException("Missing required param (dbInitialCreate, dbDropAll or dbMigrate)");
            }

            Console.WriteLine("Running DbMigrator");

            var logic = new PostgresDbMigratorLogic();
            
            switch (args[0])
            {
                case "dbInitialCreate":
                    Console.WriteLine("Running dbInitialCreate");
                    await logic.CreateUserAndDatabases(settings);
                    Console.WriteLine("Finished dbInitialCreate");
                    return 0;
                case "dbDropAll":
                    Console.WriteLine("Running dbDropAll");
                    await logic.DropAll(settings);
                    Console.WriteLine("Finished dbDropAll");
                    return 0;
                case "dbMigrate":
                    Console.WriteLine("Running dbMigrate");
                    await logic.DbMigrate(mainDbContext);
                    Console.WriteLine("Finished dbMigrate");
                    return 0;
                default:
                    throw new ApplicationException("unknown command: " + args[0]);
            }
        }
        catch (Exception ex)
        {
            MiscUtil.ConsoleError(ex);
            return 1;
        }
    }
}
