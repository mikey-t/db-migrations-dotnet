using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.RegularExpressions;

namespace MikeyT.DbMigrations;

public class MigrationScriptRunner
{
    private static ISqlPlaceholderReplacer _replacer = new DefaultSqlPlaceholderReplacer();

    /// <summary>
    /// Called from the C# migration file. Call <c>MigrationScriptRunner.SetSqlPlaceholderReplacer</c> in your DbContext <c>OnConfiguring</c> method to use a different placeholder replacer.
    /// </summary>
    /// <param name="migrationBuilder">Pass along the instance of the <c>MigrationBuilder</c> from your auto-generated <c>Migration</c> class's <c>Up</c> and <c>Down</c> methods.</param>
    /// <param name="relativePath">The path of the sql script relative to the "Scripts" directory in your migrations project. Omit the "Scripts" directory or any other prefix. For example: <c>MySqlFile.sql</c> or <c>Subdirectory/MySqlFile.sql</c>.</param>
    /// <exception cref="Exception">Throws an exception if the assembly is null or the sql file was not found in embedded resources or the sql file is empty.</exception>
    public static void RunScript(MigrationBuilder migrationBuilder, string relativePath)
    {
        // Notes on assemblies:
        // - typeof(MigrationScriptRunner).Assembly = MikeyT.DbMigrations
        // - Assembly.GetExecutingAssembly() = MikeyT.DbMigrations
        // - Assembly.GetEntryAssembly() = ef
        // - Assembly.GetCallingAssembly() = DbMigrations (or whatever the implementation project is called)
        var migrationsAssembly = Assembly.GetCallingAssembly() ?? throw new Exception(@"Cannot load sql file from embedded resource - Assembly.GetCallingAssembly() is null");
        var assemblyName = migrationsAssembly.GetName().Name ?? throw new Exception("Cannot load sql file from embedded resource - could not get name of migrationsAssembly");
        var resourceName = GetEmbeddedResourceName(assemblyName, relativePath);
        if (!migrationsAssembly.GetManifestResourceNames().Contains(resourceName))
        {
            throw new Exception($@"Could not find embedded sql file ""{resourceName}"" in assembly: {migrationsAssembly.FullName}");
        }

        using var stream = migrationsAssembly.GetManifestResourceStream(resourceName) ?? throw new Exception("Cannot read SQL file from embedded resources - stream is null");
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new Exception(@"Sql cannot be empty - if it is an intentional ""noop"", add a comment to the file and re-run the migration");
        }

        sql = _replacer.GetSqlWithPlaceholderReplacements(sql);

        migrationBuilder.Sql(sql);
    }

    /// <summary>
    /// Set what <c>ISqlPlaceholderReplacer</c> instance is used to replace placeholders in your sql scripts before they are executed. Call this in your <c>DbContext.OnConfiguring()</c> method.
    /// </summary>
    /// <param name="replacer">Any <c>ISqlPlaceholderReplacer</c> implementation is valid here.</param>
    public static void SetSqlPlaceholderReplacer(ISqlPlaceholderReplacer replacer)
    {
        _replacer = replacer;
    }

    private static string GetEmbeddedResourceName(string assemblyName, string relativePath)
    {
        // Replace all groups of slashes with a single "."
        var resourceName = Regex.Replace(relativePath, @"[\\/]+", ".").TrimStart('.');
        return $"{assemblyName}.Scripts.{resourceName}";
    }
}

public interface ISqlPlaceholderReplacer
{
    string GetSqlWithPlaceholderReplacements(string sql);
}

public class DefaultSqlPlaceholderReplacer : ISqlPlaceholderReplacer
{
    public string GetSqlWithPlaceholderReplacements(string sql)
    {
        return sql.Replace(":DB_USER", Environment.GetEnvironmentVariable("DB_USER"));
    }
}
