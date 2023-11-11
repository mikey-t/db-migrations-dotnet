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

    /// <summary>
    /// Set <c>useCallingAssembly</c> to true for unit tests.
    /// </summary>
    public DbContextFinder(bool useCallingAssembly = false)
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
            var setupType = TypeHelper.GetGenericInterfaceType(dbContext, typeof(IDbSetupContext<>));
            all.Add(new DbContextInfo(dbContext, setupType));
        }

        return all;
    }
}
