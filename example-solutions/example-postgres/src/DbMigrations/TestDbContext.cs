using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MikeyT.DbMigrations;

namespace DbMigrations;

[DbSetupClass(typeof(PostgresSetup))]
[EnvSubstitution("DB_NAME", "DB_NAME_TEST")]
public class TestDbContext : DbContext
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
