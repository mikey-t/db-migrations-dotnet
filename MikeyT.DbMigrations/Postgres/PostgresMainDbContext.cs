using Microsoft.EntityFrameworkCore;

namespace MikeyT.DbMigrations.Postgres;

public class PostgresMainDbContext : DbContext
{
    private readonly string _connectionString;
    
    public PostgresMainDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }
}
