using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MikeyT.DbMigrations.Test;

[DbSetupClass(typeof(PostgresSetup))]
public class MainDbContext : DbContext
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
