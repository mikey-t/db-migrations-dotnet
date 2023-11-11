using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MikeyT.DbMigrations;

public class PostgresMigrationsDbContext : DbContext, IDbSetupContext<PostgresSetup>
{
    public PostgresSetup GetDbSetup()
    {
        return new PostgresSetup();
    }

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
