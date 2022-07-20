namespace MikeyT.DbMigrations;

public static class DbMigratorCli
{
    public static async Task<int> Run(string[] args, DbMigratorBase dbMigratorImplementation)
    {
        try
        {
            if (args.Length == 0)
            {
                throw new Exception("Missing required param (dbInitialCreate, dbDropAll or dbMigrate)");
            }

            var command = args[0];

            Console.WriteLine($"Running DbMigrator CLI with command {command}");

            switch (command)
            {
                case "dbInitialCreate":
                    await dbMigratorImplementation.CreateUsersAndDatabases();
                    Console.WriteLine("Finished command dbInitialCreate");
                    return 0;
                case "dbDropAll":
                    await dbMigratorImplementation.DropAll();
                    Console.WriteLine("Finished command dbDropAll");
                    return 0;
                case "dbMigrate":
                    await dbMigratorImplementation.DbMigrate();
                    Console.WriteLine("Finished command dbMigrate");
                    return 0;
                default:
                    throw new Exception("unknown command: " + command);
            }
        }
        catch (Exception ex)
        {
            MiscUtil.ConsoleError(ex);
            return 1;
        }
    }
}
