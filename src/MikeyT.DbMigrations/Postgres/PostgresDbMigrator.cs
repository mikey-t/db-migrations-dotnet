using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MikeyT.DbMigrations.Postgres;

public class PostgresDbMigrator : DbMigratorBase
{
    private readonly PostgresDbMigratorSettings settings;
    private readonly DbContext DbContextForPackagedDbMigrate;
    
    public PostgresDbMigrator(DbContext dbContextForPackagedDbMigrate)
    {
        settings = new PostgresDbMigratorSettings();
        DbContextForPackagedDbMigrate = dbContextForPackagedDbMigrate;
    }
    
    public override async Task CreateUsersAndDatabases()
    {
        var rootConnectionString = settings.GetRootConnectionString();
        var logSafeRootConnectionString = settings.GetLogSafeConnectionString(rootConnectionString);
        
        Console.WriteLine($"creating databases {settings.DbName} and {settings.TestDbName} and user {settings.DbUser} using root connection string {logSafeRootConnectionString}");

        await using var connection = new NpgsqlConnection(rootConnectionString);
        if (await RoleExists(connection, settings.DbUser))
        {
            Console.WriteLine($"role {settings.DbUser} already exists, skipping");
        }
        else
        {
            await CreateRole(connection, settings.DbUser, settings.DbPassword);
            Console.WriteLine($"created role {settings.DbUser}");
        }

        if (await DbExists(connection, settings.DbName))
        {
            Console.WriteLine($"db {settings.DbName} already exists, skipping");
        }
        else
        {
            await CreateDb(connection, settings.DbName, settings.DbUser);
            Console.WriteLine($"created db {settings.DbName} with owner {settings.DbUser}");
        }

        if (await DbExists(connection, settings.TestDbName))
        {
            Console.WriteLine($"db {settings.TestDbName} already exists, skipping");
        }
        else
        {
            await CreateDb(connection, settings.TestDbName, settings.DbUser);
            Console.WriteLine($"created db {settings.TestDbName} with owner {settings.DbUser}");
        }
    }

    public override async Task DropAll()
    {
        await using var conn = new NpgsqlConnection(settings.GetRootConnectionString());
        Console.WriteLine($"dropping table {settings.DbName}");
        await DropDb(conn, settings.DbName);
        Console.WriteLine($"dropping table {settings.TestDbName}");
        await DropDb(conn, settings.TestDbName);
        Console.WriteLine($"dropping role {settings.DbUser}");
        await conn.ExecuteAsync($"DROP ROLE IF EXISTS {settings.DbUser}");
    }

    public override async Task DbMigrate()
    {
        await DbContextForPackagedDbMigrate.Database.MigrateAsync();
    }
    
    private async Task DropDb(NpgsqlConnection conn, string dbName)
    {
        // Drop existing connection before dropping DB
        await conn.ExecuteAsync(@$"SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = '{dbName}'
                    AND pid <> pg_backend_pid();DROP DATABASE IF EXISTS {dbName}");
    }

    private async Task<bool> RoleExists(NpgsqlConnection connection, string role)
    {
        return await connection.QuerySingleAsync<bool>($"select exists(SELECT FROM pg_catalog.pg_roles WHERE rolname = '{role}');");
    }

    private async Task<bool> DbExists(NpgsqlConnection connection, string dbName)
    {
        return await connection.QuerySingleAsync<bool>($"select exists(SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('{dbName}'));");
    }
    
    private async Task CreateRole(NpgsqlConnection connection, string user, string password)
    {
        await connection.ExecuteAsync($"CREATE ROLE {user} WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT NOREPLICATION CONNECTION " +
                                      $"LIMIT -1 PASSWORD '{password}';");
    }
    
    private async Task CreateDb(NpgsqlConnection connection, string dbName, string owningUser)
    {
        await connection.ExecuteAsync($"CREATE DATABASE {dbName} WITH OWNER = {owningUser} ENCODING = 'UTF8' CONNECTION LIMIT = -1;");
    }
}
