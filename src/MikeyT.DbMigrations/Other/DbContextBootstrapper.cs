using System.Reflection;

namespace MikeyT.DbMigrations;

public class DbContextBootstrapper
{
    private readonly IConsoleLogger _logger;
    private readonly ICsProjModifier _csProjModifier;

    public DbContextBootstrapper() : this(new ConsoleLogger(), new CsprojModifier(new CsprojAccessor())) { }

    public DbContextBootstrapper(IConsoleLogger logger, ICsProjModifier csProjModifier)
    {
        _logger = logger;
        _csProjModifier = csProjModifier;
    }

    public static bool IsBootstrapCommand(string[] args)
    {
        return args.Length > 0 && args[0].ToLower() == Commands.BootstrapContext;
    }

    public void Bootstrap(string[] args)
    {
        if (args.Length < 3)
        {
            throw new CliParamException("The bootstrap command expects at least 2 parameters: DbContext name and DbSetup class name");
        }

        // This is the "migrations" assembly (not the lib, but the implementation console app project it's being called in)
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new Exception("Unable to load assembly to check if DbContext type already exists");

        var contextName = ValidateDbContextName(args[1], entryAssembly);
        var setupType = ValidateDbSetupType(args[2], entryAssembly);

        EnsureClass(contextName, setupType);
        EnsureMigrationsFolder(contextName);

        if (args.Length > 2)
        {
            var scriptsSubdirectory = ValidateScriptsSubdirectory(args[3]);
            if (scriptsSubdirectory != null)
            {
                EnsureScriptsSubdirectory(scriptsSubdirectory);
            }
        }
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

    private static string? ValidateScriptsSubdirectory(string scriptsSubdirectory)
    {
        if (string.IsNullOrWhiteSpace(scriptsSubdirectory))
        {
            return null;
        }
        var invalidChars = new[] { '"', '\'', '/', '\\', ' ' };
        if (scriptsSubdirectory.Any(c => invalidChars.Contains(c)))
        {
            throw new CliParamException($@"The scripts subdirectory contains invalid characters: ""{scriptsSubdirectory}""");
        }
        return scriptsSubdirectory;
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
        if (Activator.CreateInstance(dbSetupType) is not DbSetup setup)
        {
            throw new Exception("Could not instantiate DbSetup type");
        }

        var boilerplate = setup.GetDbContextBoilerplate(dbContextName);

        _logger.WriteLine($"creating file: {path}");
        _logger.WriteLine($"New content for {filename}:\n---");
        _logger.WriteLine(boilerplate + "\n---\n");

        File.WriteAllText(path, boilerplate);
    }

    private void EnsureMigrationsFolder(string dbContextName)
    {
        string migrationsBaseDirPath = Path.Join(Environment.CurrentDirectory, "Migrations");

        if (!Directory.Exists(migrationsBaseDirPath))
        {
            _logger.WriteLine($"creating base migrations directory: {migrationsBaseDirPath}");
            Directory.CreateDirectory(migrationsBaseDirPath);
        }

        string relativeMigrationsPath = $"Migrations/{dbContextName}Migrations";

        string migrationsDirPath = Path.Join(Environment.CurrentDirectory, relativeMigrationsPath);

        if (Directory.Exists(migrationsDirPath))
        {
            _logger.WriteLine($"migrations directory already exists, skipping: {migrationsDirPath}");
        }
        else
        {
            _logger.WriteLine($"creating migrations subdirectory for new context: {migrationsDirPath}");
            Directory.CreateDirectory(migrationsDirPath);
        }

        string? csprojPath = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").FirstOrDefault();
        if (csprojPath == null)
        {
            _logger.Warn("No csproj file found in current directory - you will have the add the Folder reference manually for the new migrations directory:");
            _logger.WriteLine($@"<ItemGroup><Folder Include=""{relativeMigrationsPath}"" /></ItemGroup>");
            return;
        }

        _logger.WriteLine($"adding Folder reference to csproj file: {csprojPath}");

        _csProjModifier.EnsureFolderInclude(csprojPath, relativeMigrationsPath);
    }

    private void EnsureScriptsSubdirectory(string scriptsSubdirectory)
    {
        string scriptsBaseDirPath = Path.Join(Environment.CurrentDirectory, "Scripts");

        if (!Directory.Exists(scriptsBaseDirPath))
        {
            _logger.WriteLine($"creating Scripts directory: {scriptsBaseDirPath}");
            Directory.CreateDirectory(scriptsBaseDirPath);
        }

        string scriptsSubdirectoryPath = Path.Join(Environment.CurrentDirectory, "Scripts", scriptsSubdirectory);

        if (Directory.Exists(scriptsSubdirectoryPath))
        {
            _logger.WriteLine($"scripts subdirectory already exists, skipping: {scriptsSubdirectoryPath}");
        }
        else
        {
            _logger.WriteLine($"creating scripts subdirectory: {scriptsSubdirectoryPath}");
            Directory.CreateDirectory(scriptsSubdirectoryPath);
        }
    }
}
