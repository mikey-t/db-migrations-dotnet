using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MikeyT.DbMigrations;

public interface IDbContextFinder
{
    public List<DbContextInfo> GetAllDbContextInfos();
}

public class DbContextFinder : IDbContextFinder
{
    private bool _useCallingAssembly = false;

    public DbContextFinder() : this(false) { }

    // Set useCallingAssembly to true for unit tests
    public DbContextFinder(bool useCallingAssembly)
    {
        _useCallingAssembly = useCallingAssembly;
    }

    public List<DbContextInfo> GetAllDbContextInfos()
    {
        var all = new List<DbContextInfo>();

        Assembly assembly;
        if (_useCallingAssembly)
        {
            assembly = Assembly.GetCallingAssembly();
        }
        else
        {
            assembly = Assembly.GetEntryAssembly() ?? throw new Exception("Assembly.GetEntryAssembly() returned null - cannot scan for DbContext types");
        }

        var dbContextTypes = assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(DbContext)) && !type.IsAbstract)
            .ToArray();

        foreach (var dbContext in dbContextTypes)
        {
            // Note that the setupType might be null - take action on this elsewhere
            var setupType = dbContext.GetCustomAttribute<DbSetupClassAttribute>()?.SetupClass;

            var envSubstitutions = new List<EnvSubstitution>();

            var envSubAttributes = dbContext.GetCustomAttributes<EnvSubstitutionAttribute>();
            foreach (var subAttribute in envSubAttributes)
            {
                envSubstitutions.Add(new EnvSubstitution(subAttribute.FromEnvKey, subAttribute.ToEnvKey));
            }

            all.Add(new DbContextInfo(dbContext, setupType, envSubstitutions));
        }

        return all;
    }
}
