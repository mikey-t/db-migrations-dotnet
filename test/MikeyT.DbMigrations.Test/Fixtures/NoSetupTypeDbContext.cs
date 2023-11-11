using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MikeyT.DbMigrations.Test.Fixtures;

// This test fixture class does not inherit from IDbMigrationsContext. It should still be found
// by DbContextFinder, but it will not have metadata about the DbSetup type.
public class NoSetupTypeDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DotEnv.LoadStatic();
        var settings = new PostgresSettings();
        var connectionString = settings.GetMigrationsConnectionString();
        Console.WriteLine("Using connection string: " + settings.GetLogSafeConnectionString(connectionString));
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        MigrationScriptRunner.SetSqlPlaceholderReplacer(new DefaultSqlPlaceholderReplacer());
    }
}
