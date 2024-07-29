using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace example_basic_concept;

public class MyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = "Server=127.0.0.1,1430;Database=dbmigrationsexample;User Id=sa;Password=Abc1234!;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString);
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }
}
