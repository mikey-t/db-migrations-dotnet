using System.Reflection;

namespace MikeyT.DbMigrations;

public class DbContextBootstrapper
{
    private readonly IConsoleLogger _logger;

    public DbContextBootstrapper() : this(new ConsoleLogger()) { }

    public DbContextBootstrapper(IConsoleLogger logger)
    {
        _logger = logger;
    }

    public static bool IsBootstrapCommand(string[] args)
    {
        return args.Length > 0 && args[0].ToLower() == Commands.BootstrapContext;
    }

    public void Bootstrap(string[] args)
    {
        if (args.Length < 3)
        {
            throw new CliParamException("The bootstrap command expects 2 parameters: DbContext name and DbSetup class name");
        }

        // This is the "migrations" assembly (not the lib, but the implementation console app project it's being called in)
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new Exception("Unable to load assembly to check if DbContext type already exists");

        var contextName = ValidateDbContextName(args[1], entryAssembly);
        var setupType = ValidateDbSetupType(args[2], entryAssembly);

        EnsureClass(contextName, setupType);
        EnsureMigrationsFolder(contextName);
    }

    private string ValidateDbContextName(string contextName, Assembly entryAssembly)
    {
        var trimmedContextName = contextName.Trim();

        _logger.WriteLine($"checking if DbContext name is valid: {trimmedContextName}");

        if (!ClassNameValidator.IsValidClassName(trimmedContextName))
        {
            throw new CliParamException($@"The class name passed for the new DbContext is invalid: ""{trimmedContextName}""");
        }

        if (!trimmedContextName.EndsWith("DbContext"))
        {
            throw new CliParamException(@$"The class name passed for the new DbContext must end with ""DbContext"": ""{trimmedContextName}""");
        }

        if (trimmedContextName == "DbContext")
        {
            throw new CliParamException(@$"The class name passed for the new DbContext must not literally be ""DbContext""");
        }

        if (TypeHelper.DoesExactlyOneClassExist(trimmedContextName, entryAssembly))
        {
            throw new Exception($"The DbContext type already exists: {trimmedContextName}");
        }

        _logger.Success($"DbContext name is valid: {trimmedContextName}");

        return trimmedContextName;
    }

    private Type ValidateDbSetupType(string setupTypeName, Assembly entryAssembly)
    {
        _logger.WriteLine($"checking if DbSetup type is valid: {setupTypeName}");

        if (!ClassNameValidator.IsValidClassName(setupTypeName))
        {
            throw new CliParamException(@$"The class name passed for the DbSetup type is invalid: ""{setupTypeName}""");
        }


        var libAssembly = Assembly.Load(GlobalConstants.AssemblyName) ?? throw new Exception($"Unable to load assembly {GlobalConstants.AssemblyName} to retrieve DbSetup type information");

        var setupType = GetSetupType(setupTypeName, entryAssembly, libAssembly) ?? throw new CliParamException($@"Unable to find DbSetup type: ""{setupTypeName}""");

        _logger.Success($"Found DbSetup type: {setupType.FullName}");

        return setupType;
    }

    // Order of precedence: entry assembly -> lib assembly
    private Type? GetSetupType(string className, Assembly entryAssembly, Assembly libAssembly)
    {
        var assemblies = new[] { entryAssembly, libAssembly };
        foreach (var assembly in assemblies)
        {
            Type? match = TypeHelper.GetTypeIfExactlyOne(className, assembly);
            if (match != null)
            {
                if (match.IsSubclassOf(typeof(DbSetup)))
                {
                    return match;
                }
                else
                {
                    _logger.Warn($@"Found type ""{match.FullName}"" in assembly ""{entryAssembly.GetName().Name}"" but it is not a subclass of DbSetup");
                }
            }
        }
        return null;
    }

    private void EnsureClass(string dbContextName, Type dbSetupType)
    {
        string filename = dbContextName + ".cs";
        string path = Path.Join(Environment.CurrentDirectory, filename);
        if (File.Exists(path))
        {
            throw new Exception($"Cannot write new file - a file with the DbContext name already exists: {path}");
        }

        _logger.WriteLine($"Instantiating new DbSetup instance ({dbSetupType.Name}) to retrieve boilerplate");
        var setup = Activator.CreateInstance(dbSetupType);
        if (setup == null)
        {
            _logger.Info("setup ok");
        }
        else
        {
            _logger.Info("setup is NULL");
        }
        
        
        // _logger.WriteLine($"creating file: {path}");
        
    }

    private void EnsureMigrationsFolder(string dbContext)
    {

    }
}
