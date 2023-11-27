using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MikeyT.DbMigrations;

public class PostgresMigrationsDbContext : DbContext, IDbSetupContext<PostgresSetup>
{
    public virtual PostgresSetup GetDbSetup()
    {
        return new PostgresSetup();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var settings = GetDbSetup().Settings;
        var connectionString = settings.GetMigrationsConnectionString();
        Console.WriteLine("Using connection string: " + settings.GetLogSafeConnectionString(connectionString));
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        MigrationScriptRunner.SetSqlPlaceholderReplacer(new DefaultSqlPlaceholderReplacer());
    }
}
