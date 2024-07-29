using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace example_basic_concept;

public static class MigrationBuilderExtensions
{
    public static void RunScript(this MigrationBuilder migrationBuilder, string scriptFilename)
    {
        var migrationsAssembly = Assembly.GetCallingAssembly();
        var assemblyName = migrationsAssembly.GetName().Name;
        var resourceName = $"{assemblyName!.Replace("-", "_")}.Scripts.{scriptFilename}";

        using var stream = migrationsAssembly.GetManifestResourceStream(resourceName) ?? throw new Exception("Cannot read SQL file from embedded resources - stream is null");
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        migrationBuilder.Sql(sql);
    }
}
