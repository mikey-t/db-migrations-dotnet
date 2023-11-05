using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MikeyT.DbMigrations.Test;

// This class is not annotated with DbSetupClassAttribute, so should not be found by the DbContextFinder
public class NoSetupTypeDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DotEnv.LoadStatic();
        var settings = new PostgresSettings(GetType());
        var connectionString = settings.GetMigrationsConnectionString();
        Console.WriteLine("Using connection string: " + settings.GetLogSafeConnectionString(connectionString));
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        MigrationScriptRunner.SetSqlPlaceholderReplacer(new DefaultSqlPlaceholderReplacer());
    }
}
