using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MikeyT.DbMigrations;

public class DbSetupCli
{
    private readonly IConsoleLogger _logger;
    private readonly IDbContextFinder _dbContextFinder;
    private readonly bool _rethrowUnhandled;

    private const string Help = @"
Usage: dotnet run <command> [options]

Commands:
  setup       Creates the database and roles for the specified DbContext classes.
  teardown    Drops the database and roles for the specified DbContext classes.
  list        List DbContext classes in project along with DbSetup types they use.
  bootstrap   Unlike other commands, pass a single DbContext class name and the name of the
              DbSetup class implementation for the database type you want. For this command
              the class names should be the full case sensitive names of the classes.
  help        Displays this help message.

Options:
  [DbContextNames]    Space-separated names of DbContext classes to operate on.
                      Names are case-insensitive and the 'DbContext' postfix is optional.

Examples:
  dotnet run setup application user                   Sets up the databases and user roles
                                                      for ApplicationDbContext and UserDbContext.
  dotnet run teardown application                     Tears down the ApplicationDbContext.
  dotnet run bootstrap MainDbContext PostgresSetup    Bootstraps a new DbContext class.
  dotnet run help                                     Displays this help message.
";

    public DbSetupCli() : this(new ConsoleLogger(), new DbContextFinder()) { }

    public DbSetupCli(IConsoleLogger logger, IDbContextFinder dbContextFinder, bool rethrowUnhandled = false)
    {
        _logger = logger;
        _dbContextFinder = dbContextFinder;
        _rethrowUnhandled = rethrowUnhandled;
    }

    public async Task<int> Run(string[] args)
    {
        try
        {
            if (DbContextBootstrapper.IsBootstrapCommand(args))
            {
                new DbContextBootstrapper().Bootstrap(args);
                return 0;
            }

            var setupArgs = new DbSetupArgsParser(_dbContextFinder).GetDbSetupArgs(args);

            switch (setupArgs.Command)
            {
                case Commands.List:
                    ListDbContextInfos();
                    break;
                case Commands.Setup:
                    await OperateOnDbContexts(Commands.Setup, setupArgs);
                    break;
                case Commands.Teardown:
                    await OperateOnDbContexts(Commands.Teardown, setupArgs);
                    break;
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

            if (_rethrowUnhandled)
            {
                throw;
            }

            return 1;
        }
    }

    public void ListDbContextInfos()
    {
        var contextsInfos = _dbContextFinder.GetAllDbContextInfos();
        if (contextsInfos.Count == 0)
        {
            _logger.Warn("No DbContext classes found");
            return;
        }

        var headers = new List<string>() { "DbContext Name", "DbSetup Type" };
        var rows = new List<List<string>>();
        foreach (var contextInfo in contextsInfos)
        {
            var dbContextName = contextInfo.DbContextType.Name;

            var dbSetupTypeName = contextInfo.SetupType?.Name;
            if (dbSetupTypeName == null)
            {
                dbSetupTypeName = "⚠️ (Does not inherit from IDbSetupContext)";
            }
            else
            {
                dbSetupTypeName = "✔️ " + dbSetupTypeName;
            }

            rows.Add(new List<string>() { dbContextName, dbSetupTypeName });
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
                _logger.Warn($@"The DbContext type ""{dbContextTypeName}"" does not inherit from IDbSetupContext - skipping");
                continue;
            }

            _logger.Info($@"Running {command} for DbContext ""{dbContextTypeName}"" using DbSetupType ""{setupTypeName}""");

            var contextInstance = Activator.CreateInstance(dbContextInfo.DbContextType)
                ?? throw new Exception("Unable to instantiate DbContext type");

            Type iDbSetupContextType = typeof(IDbSetupContext<>);
            Type[] iDbSetupContextTypeArgs = { dbContextInfo.SetupType };
            Type iDbSetupContextTypeWithGenericArgs = iDbSetupContextType.MakeGenericType(iDbSetupContextTypeArgs);

            MethodInfo getDbSetupMethod = iDbSetupContextTypeWithGenericArgs.GetMethod("GetDbSetup", Array.Empty<Type>())
                ?? throw new Exception($@"Unable to dynamically get IDbSetupContext method ""GetDbSetup""");

            if (getDbSetupMethod.Invoke(contextInstance, null) is not DbSetup dbSetup)
            {
                throw new Exception($@"Unable to dynamically invoke IDbSetupContext method ""GetDbSetup""");
            }

            if (command == Commands.Setup)
            {
                await dbSetup.Setup();
            }
            else
            {
                await dbSetup.Teardown();
            }
        }
    }
}
