using System.Reflection;

namespace MikeyT.DbMigrations;

/// <summary>
/// Implementations should not be including anything in their constructors that would cause dynamic instantiation to fail.
/// Instead, use the <c>Load</c> method to perform any initialization such as loading environment variables.
/// </summary>
public abstract class DbSettings
{
    private bool _loaded = false;

    /// <summary>
    /// Implementation must provide a method to load environment variables or perform other initialization.
    /// This will be called before running setup or teardown commands.
    /// </summary>
    public abstract void Load();

    public string GetMigrationsConnectionString()
    {
        EnsureLoaded();
        return GetMigrationsConnectionStringImpl();
    }

    /// <summary>
    /// Implementation must provide a connection string that EntityFramework can use to access the correct database
    /// and change any schema necessary.
    /// </summary>
    protected abstract string GetMigrationsConnectionStringImpl();

    public string GetDbSetupConnectionString()
    {
        EnsureLoaded();
        return GetDbSetupConnectionStringImpl();
    }

    /// <summary>
    /// Implementation must provide a connection string for an account that has the necessary privileges to create
    /// and drop databases and users.
    /// </summary>
    protected abstract string GetDbSetupConnectionStringImpl();

    /// <summary>
    /// Returns a connection string with redacted values for any string properties or fields in the implementation class that
    /// are decorated with the <c>[DoNotLog]</c> attribute, or that have "password" anywhere in their names (case insensitive).
    /// </summary>
    public string GetLogSafeConnectionString(string connectionString)
    {
        var type = GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var member in fields.Cast<MemberInfo>().Concat(properties))
        {
            if (member is FieldInfo field && field.FieldType != typeof(string) ||
                member is PropertyInfo property && property.PropertyType != typeof(string))
            {
                continue;
            }

            bool hasPasswordInName = member.Name.ToLower().Contains("password");
            bool hasDoNotLogAttribute = member.GetCustomAttribute<DoNotLogAttribute>() != null;

            if (!hasPasswordInName && !hasDoNotLogAttribute)
            {
                continue;
            }

            string value = (member is FieldInfo fieldInfo
               ? fieldInfo.GetValue(this)
               : ((PropertyInfo)member).GetValue(this)) as string ?? string.Empty;

            if (!string.IsNullOrEmpty(value))
            {
                connectionString = connectionString.Replace(value, "*****");
            }
        }

        return connectionString;
    }

    private void EnsureLoaded()
    {
        if (!_loaded)
        {
            Load();
            _loaded = true;
        }
    }
}
