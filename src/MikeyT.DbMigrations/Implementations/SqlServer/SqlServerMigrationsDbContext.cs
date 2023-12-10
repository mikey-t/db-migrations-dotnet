using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace MikeyT.DbMigrations;

public class SqlServerMigrationsDbContext : DbContext, IDbSetupContext<SqlServerSetup>
{
    public virtual SqlServerSetup GetDbSetup()
    {
        return new SqlServerSetup();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var settings = GetDbSetup().Settings;
        var connectionString = settings.GetMigrationsConnectionString();
        Console.WriteLine("Using connection string: " + settings.GetLogSafeConnectionString(connectionString));
        optionsBuilder.UseSqlServer(connectionString);
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        MigrationScriptRunner.SetSqlPlaceholderReplacer(new DefaultSqlPlaceholderReplacer());
    }
}
