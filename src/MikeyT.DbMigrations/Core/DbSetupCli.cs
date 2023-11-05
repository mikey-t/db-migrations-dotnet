namespace MikeyT.DbMigrations;

public class DbSetupCli
{
    private readonly IConsoleLogger _logger;
    private readonly IDbContextFinder _dbContextFinder;

    private const string Help = @"
Usage: dotnet run <command> [options]

Commands:
  setup       Creates the database and roles for the specified DbContext classes.
  teardown    Drops the database and roles for the specified DbContext classes.
  help        Displays this help message.

Options:
  [DbContextNames]    Space-separated names of DbContext classes to operate on.
                      Names are case-insensitive and the 'DbContext' postfix is optional.

Examples:
  dotnet run setup application user    Sets up the databases and user roles for ApplicationDbContext and UserDbContext.
  dotnet run teardown application      Tears down the ApplicationDbContext.
  dotnet run help                      Displays this help message.
";

    public DbSetupCli() : this(new ConsoleLogger(), new DbContextFinder()) { }

    public DbSetupCli(IConsoleLogger logger, IDbContextFinder dbContextFinder)
    {
        _logger = logger;
        _dbContextFinder = dbContextFinder;
    }

    public async Task<int> Run(string[] args)
    {
        try
        {
            var setupArgs = new DbSetupArgsParser().GetDbSetupArgs(args);

            switch (setupArgs.Command)
            {
                case Commands.List:
                    ListDbContextInfos();
                    return 0;
                case Commands.Setup:
                    await OperateOnDbContexts(Commands.Setup, setupArgs);
                    return 0;
                case Commands.Teardown:
                    await OperateOnDbContexts(Commands.Teardown, setupArgs);
                    return 0;
            }
            return 0;
        }
        catch (CliParamException paramEx)
        {
            _logger.Error(paramEx.Message);
            _logger.WriteLine(Help);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            return 1;
        }
    }

    public void ListDbContextInfos()
    {
        var contextsInfos = _dbContextFinder.GetAllDbContextInfos();
        if (contextsInfos.Count == 0)
        {
            _logger.Warn("⚠️ No DbContext classes found");
            return;
        }

        var headers = new List<string>() { "DbContext Name", "DbSetupType Name", "Env Substitutions" };
        var rows = new List<List<string>>();
        foreach (var contextInfo in contextsInfos)
        {
            var dbContextName = contextInfo.DbContextType.Name;

            var dbSetupTypeName = contextInfo.SetupType?.Name;
            if (dbSetupTypeName == null)
            {
                dbSetupTypeName = "⚠️ (missing DbSetupType attribute)";
            }
            else
            {
                dbSetupTypeName = "✔️ " + dbSetupTypeName;
            }

            var envSubMappingStrings = new List<string>();
            foreach (var sub in contextInfo.EnvSubstitutions)
            {
                envSubMappingStrings.Add($"{sub.FromEnvKey} -> {sub.ToEnvKey}");
            }
            var envSubsString = string.Join(", ", envSubMappingStrings);

            rows.Add(new List<string>() { dbContextName, dbSetupTypeName, envSubsString });
        }
        _logger.WriteLine(Environment.NewLine + TableGenerator.Generate(headers, rows) + Environment.NewLine);
    }

    private async Task OperateOnDbContexts(string command, DbSetupArgs setupArgs)
    {
        if (command != Commands.Setup && command != Commands.Teardown)
        {
            throw new Exception($"Unknown command: {command}");
        }

        foreach (var dbContextInfo in setupArgs.DbContextInfos)
        {
            var dbContextTypeName = dbContextInfo.DbContextType.Name;
            var setupTypeName = dbContextInfo.SetupType?.Name;

            if (dbContextInfo.SetupType == null)
            {
                _logger.Warn($@"The DbContext type ""{dbContextTypeName}"" does not have a SetupType attribute - skipping");
                continue;
            }

            _logger.Info($@"Running {command} for DbContext ""{dbContextTypeName}"" using DbSetupType ""{setupTypeName}""");

            if (Activator.CreateInstance(dbContextInfo.SetupType, dbContextInfo.DbContextType) is not DbSetup setupInstance)
            {
                throw new Exception($@"The SetupType provided was instantiated but it does not appear to be derived from the DbSetup class: ""{setupTypeName}""");
            }

            if (setupInstance is null)
            {
                throw new Exception($@"Unable to instantiate DbSetup type ""{setupTypeName}"" - Activator.CreateInstance returned null");
            }

            if (command == Commands.Setup)
            {
                await setupInstance.Setup();
            }
            else
            {
                await setupInstance.Teardown();
            }
        }
    }
}
