using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MikeyT.DbMigrations;

public class PostgresSetup : DbSetup
{
    private readonly PostgresSettings _settings;
    private readonly IConsoleLogger _consoleLogger;

    public PostgresSetup(Type dbContextType) : this(dbContextType, new ConsoleLogger()) { }

    public PostgresSetup(Type dbContextType, IConsoleLogger consoleLogger)
    {
        _settings = new PostgresSettings(dbContextType);
        _consoleLogger = consoleLogger;
    }

    public override async Task Setup()
    {
        var rootConnectionString = _settings.GetRootConnectionString();
        var logSafeRootConnectionString = _settings.GetLogSafeConnectionString(rootConnectionString);

        _consoleLogger.WriteLine($"creating database {_settings.DbName} and user {_settings.DbUser} using root connection string: {logSafeRootConnectionString}");

        await using var connection = new NpgsqlConnection(rootConnectionString);

        await ThrowIfUnsafeRoleOrDbName(connection, _settings.DbUser, _settings.DbName);

        if (await RoleExists(connection, _settings.DbUser))
        {
            _consoleLogger.WriteLine($"role {_settings.DbUser} already exists, skipping");
        }
        else
        {
            await CreateRole(connection, _settings.DbUser, _settings.DbPassword);
            _consoleLogger.WriteLine($"created role {_settings.DbUser}");
        }

        if (await DbExists(connection, _settings.DbName))
        {
            _consoleLogger.WriteLine($"db {_settings.DbName} already exists, skipping");
        }
        else
        {
            await CreateDb(connection, _settings.DbName, _settings.DbUser);
            _consoleLogger.WriteLine($"created db {_settings.DbName} with owner {_settings.DbUser}");
        }
    }

    public override async Task Teardown()
    {
        var rootConnectionString = _settings.GetRootConnectionString();
        var logSafeRootConnectionString = _settings.GetLogSafeConnectionString(rootConnectionString);

        _consoleLogger.WriteLine($"dropping database {_settings.DbName} and user {_settings.DbUser} using root connection string: {logSafeRootConnectionString}");

        await using var connection = new NpgsqlConnection(rootConnectionString);

        _consoleLogger.WriteLine($"dropping database: {_settings.DbName}");
        await DropDb(connection, _settings.DbName);

        _consoleLogger.WriteLine($"dropping role {_settings.DbUser}");
        await DropRole(connection, _settings.DbUser);
    }

    private static async Task ThrowIfUnsafeRoleOrDbName(NpgsqlConnection connection, string roleName, string dbName)
    {
        var safeRoleName = await connection.QueryFirstOrDefaultAsync<string>("SELECT quote_ident(@RoleName);", new { RoleName = roleName });
        if (safeRoleName != roleName)
        {
            throw new Exception($"The role could not be deleted because the role name did not pass the quote_ident test: ${roleName}");
        }

        var safeDbName = await connection.QueryFirstOrDefaultAsync<string>("SELECT quote_ident(@DbName);", new { DbName = dbName });
        if (safeDbName != dbName)
        {
            throw new Exception($"The database could not be deleted because the database name did not pass the quote_ident test: ${roleName}");
        }
    }

    private async Task DropDb(NpgsqlConnection connection, string dbName)
    {
        var dbExists = await DbExists(connection, dbName);

        if (!dbExists)
        {
            _consoleLogger.WriteLine("database does not exist - skipping");
            return;
        }

        // Terminate existing connections to the database before dropping
        await connection.ExecuteAsync($"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}' AND pid <> pg_backend_pid();");
        await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {dbName}");
    }

    private static async Task<bool> RoleExists(NpgsqlConnection connection, string roleName)
    {
        return await connection.QuerySingleAsync<bool>($"select exists(SELECT FROM pg_catalog.pg_roles WHERE rolname = '{roleName}');");
    }

    private static async Task<bool> DbExists(NpgsqlConnection connection, string dbName)
    {
        return await connection.QuerySingleAsync<bool>($"select exists(SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('{dbName}'));");
    }

    private static async Task CreateRole(NpgsqlConnection connection, string user, string password)
    {
        await connection.ExecuteAsync($"CREATE ROLE {user} WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT NOREPLICATION CONNECTION " +
                                      $"LIMIT -1 PASSWORD '{password}';");
    }

    private static async Task CreateDb(NpgsqlConnection connection, string dbName, string owningUser)
    {
        await connection.ExecuteAsync($"CREATE DATABASE {dbName} WITH OWNER = {owningUser} ENCODING = 'UTF8' CONNECTION LIMIT = -1;");
    }

    private async Task DropRole(NpgsqlConnection connection, string roleName)
    {
        var roleExists = await RoleExists(connection, roleName);
        if (!roleExists)
        {
            _consoleLogger.WriteLine("no role found - skipping");
            return;
        }

        var roleHasDependentObjects = await RoleHasDependentObjects(connection, roleName);
        if (roleHasDependentObjects)
        {
            _consoleLogger.WriteLine("the role has dependent database(s) and will not be dropped (it will be dropped when the last database is dropped if you passed multiple databases to operate on)");
            return;
        }

        await connection.ExecuteAsync($"DROP ROLE IF EXISTS {roleName}");
    }

    private static async Task<bool> RoleHasDependentObjects(NpgsqlConnection connection, string roleName)
    {
        var query = $@"SELECT EXISTS (
                        SELECT 1 FROM pg_database
                        WHERE datdba = (SELECT oid FROM pg_roles WHERE rolname = @RoleName)
                    );";

        return await connection.QueryFirstOrDefaultAsync<bool>(query, new { RoleName = roleName });
    }
}
