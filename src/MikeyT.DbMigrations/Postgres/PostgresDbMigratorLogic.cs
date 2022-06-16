using Npgsql;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace MikeyT.DbMigrations.Postgres;

public class PostgresDbMigratorLogic
{
    public async Task CreateUserAndDatabases(PostgresDbMigratorSettings settings)
    {
        var user = settings.GetDbUser();
        var pass = settings.GetDbPass();
        var dbName = settings.GetDbName();
        var testDbName = settings.GetTestDbName();
        var rootConnectionString = settings.GetRootConnectionString();
        var logSafeConnString = settings.GetLogSafeConnectionString(rootConnectionString);

        Console.WriteLine($"Creating databases {dbName} and {testDbName} for user {user} using root connection string {logSafeConnString}");

        await using var conn = new NpgsqlConnection(rootConnectionString);
        if (RoleExists(conn, user))
        {
            Console.WriteLine($"role {user} already exists, skipping");
        }
        else
        {
            await conn.ExecuteAsync(
                $"CREATE ROLE {user} WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT NOREPLICATION CONNECTION LIMIT -1 PASSWORD '{pass}';");
        }

        if (DbExists(conn, dbName))
        {
            Console.WriteLine($"db {dbName} already exists, skipping");
        }
        else
        {
            await conn.ExecuteAsync($"CREATE DATABASE {dbName} WITH OWNER = {user} ENCODING = 'UTF8' CONNECTION LIMIT = -1;");
            Console.WriteLine($"created db {dbName}");
        }

        if (DbExists(conn, testDbName))
        {
            Console.WriteLine($"db {testDbName} already exists, skipping");
        }
        else
        {
            await conn.ExecuteAsync($"CREATE DATABASE {testDbName} WITH OWNER = {user} ENCODING = 'UTF8' CONNECTION LIMIT = -1;");
            Console.WriteLine($"created db {testDbName}");
        }
    }

    public async Task DropAll(PostgresDbMigratorSettings settings)
    {
        await using var conn = new NpgsqlConnection(settings.GetRootConnectionString());
        await DropDb(conn, settings.GetDbName());
        await DropDb(conn, settings.GetTestDbName());
        await conn.ExecuteAsync($"DROP ROLE IF EXISTS {settings.GetDbUser()}");
    }

    public async Task DbMigrate(DbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();
    }

    private async Task DropDb(NpgsqlConnection conn, string dbName)
    {
        // Drop existing connection before dropping DB
        await conn.ExecuteAsync(@$"SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = '{dbName}'
                    AND pid <> pg_backend_pid();DROP DATABASE IF EXISTS {dbName}");
    }

    private bool RoleExists(NpgsqlConnection connection, string role)
    {
        return connection.QuerySingle<bool>($"select exists(SELECT FROM pg_catalog.pg_roles WHERE rolname = '{role}');");
    }

    private bool DbExists(NpgsqlConnection connection, string dbName)
    {
        return connection.QuerySingle<bool>($"select exists(SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('{dbName}'));");
    }
}
