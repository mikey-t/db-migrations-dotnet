using System.Reflection;

namespace MikeyT.DbMigrations;

public abstract class DbSettings
{
    /// <summary>
    /// Implementation must provide a connection string that EntityFramework can use to access the correct database
    /// and change any schema necessary.
    /// </summary>
    public abstract string GetMigrationsConnectionString();

    /// <summary>
    /// Implementation must provide a connection string for an account that has the necessary privileges to create
    /// and drop databases and users.
    /// </summary>
    public abstract string GetDbSetupConnectionString();

    /// <summary>
    /// Returns a connection string with redacted values for any string properties in the implementation class that
    /// are decorated with the <c>[DoNotLog]</c> attribute.
    /// </summary>
    public string GetLogSafeConnectionString(string connectionString)
    {
        foreach (var property in GetType().GetProperties())
        {
            if (property.PropertyType != typeof(string))
            {
                continue;
            }

            var propNameHasPassword = property.Name.ToLower().Contains("password");
            var doNotLogAttribute = property.GetCustomAttribute<DoNotLogAttribute>();
            if (doNotLogAttribute == null && !propNameHasPassword)
            {
                continue;
            }

            if (property.GetValue(this) is string value && !string.IsNullOrEmpty(value))
            {
                connectionString = connectionString.Replace(value, "*****");
            }
        }

        return connectionString;
    }
}
