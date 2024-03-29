using System.Collections.Immutable;

namespace MikeyT.DbMigrations;

public class DbSetupArgsParser
{
    private static readonly string[] AvailableCommands = { Commands.Setup, Commands.Teardown, Commands.List };

    private readonly IDbContextFinder _dbContextFinder;

    public DbSetupArgsParser() : this(new DbContextFinder()) { }

    public DbSetupArgsParser(IDbContextFinder dbContextFinder)
    {
        _dbContextFinder = dbContextFinder;
    }

    public DbSetupArgs GetDbSetupArgs(string[] args)
    {
        if (args.Length == 0)
        {
            throw new CliParamException($"Missing required first param must be one of the following: {Commands.AllCommandsCommaSeparated}");
        }

        var command = args[0].ToLower();
        if (!AvailableCommands.Contains(command))
        {
            throw new CliParamException($@"Unknown command ""{command}"" - available commands: {Commands.AllCommandsCommaSeparated}");
        }

        if (command == Commands.List)
        {
            return new DbSetupArgs(command, new List<DbContextInfo>());
        }

        const string missingContextNames = "Missing required DbContext names to operate on";
        if (args.Length < 2)
        {
            throw new CliParamException(missingContextNames);
        }

        var allDbContextInfos = _dbContextFinder.GetAllDbContextInfos();

        var matchingDbContextInfos = new List<DbContextInfo>();

        for (var i = 1; i < args.Length; i++)
        {
            matchingDbContextInfos.Add(TryFindingDbContextInfoMatch(args[i], allDbContextInfos));
        }

        return new DbSetupArgs(command, matchingDbContextInfos);
    }

    // Case insensitive, does not require "DbContext" on the end
    private DbContextInfo TryFindingDbContextInfoMatch(string name, List<DbContextInfo> allDbContextInfos)
    {
        var dbContextInfo = allDbContextInfos.FirstOrDefault(x => x.DbContextType.Name.ToLower() == name.ToLower());
        if (dbContextInfo != null)
        {
            return dbContextInfo;
        }

        dbContextInfo = allDbContextInfos.FirstOrDefault(x => x.DbContextType.Name.ToLower().Replace("dbcontext", "") == name.ToLower());
        if (dbContextInfo != null)
        {
            return dbContextInfo;
        }

        throw new CliParamException($@"Could not find DbContext with name ""{name}"" - try using the ""list"" command to get a list of all the available DbContext classes in the project");
    }
}
