using Dapper;
using Npgsql;

namespace MikeyT.DbMigrations;

public class PostgresSetup : DbSetup
{
    private readonly PostgresSettings _settings;

    public PostgresSetup() : this(new PostgresSettings(), new ConsoleLogger()) { }

    public PostgresSetup(PostgresEnvKeys envKeys) : this(new PostgresSettings(envKeys), new ConsoleLogger()) { }

    public PostgresSetup(PostgresSettings settings) : this(settings, new ConsoleLogger()) { }

    public PostgresSetup(PostgresSettings settings, IConsoleLogger logger) : base(settings, logger)
    {
        _settings = settings;
    }

    public override async Task Setup()
    {
        var connectionString = _settings.GetDbSetupConnectionString();
        var logSafeConnectionString = _settings.GetLogSafeConnectionString(connectionString);

        var dbName = _settings.GetDbName();
        var dbUser = _settings.GetDbUser();
        var dbPassword = _settings.GetDbPassword();

        Logger.WriteLine($"creating database {dbName} and user {dbUser} using DbSetup connection string: {logSafeConnectionString}");

        await using var connection = new NpgsqlConnection(connectionString);

        await ThrowIfUnsafeRoleOrDbName(connection, dbUser, dbName);

        if (await RoleExists(connection, dbUser))
        {
            Logger.WriteLine($"role {dbUser} already exists, skipping");
        }
        else
        {
            await CreateRole(connection, dbUser, dbPassword);
            Logger.WriteLine($"created role {dbUser}");
        }

        if (await DbExists(connection, dbName))
        {
            Logger.WriteLine($"db {dbName} already exists, skipping");
        }
        else
        {
            await CreateDb(connection, dbName, dbUser);
            Logger.WriteLine($"created db {dbName} with owner {dbUser}");
        }
    }

    public override async Task Teardown()
    {
        var connectionString = _settings.GetDbSetupConnectionString();
        var logSafeConnectionString = _settings.GetLogSafeConnectionString(connectionString);

        var dbName = _settings.GetDbName();
        var dbUser = _settings.GetDbUser();

        Logger.WriteLine($"dropping database {dbName} and user {dbUser} using DbSetup connection string: {logSafeConnectionString}");

        await using var connection = new NpgsqlConnection(connectionString);

        Logger.WriteLine($"dropping database: {dbName}");
        await DropDb(connection, dbName);

        Logger.WriteLine($"dropping role {dbUser}");
        await DropRole(connection, dbUser);
    }

    public override string GetDbContextBoilerplate(string dbContextName)
    {
        string boilerplate = @"using MikeyT.DbMigrations;

namespace DbMigrations;

public class PlaceholderDbContext : PostgresMigrationsDbContext { }
";

        return boilerplate.Replace("PlaceholderDbContext", dbContextName);
    }

    private async Task ThrowIfUnsafeRoleOrDbName(NpgsqlConnection connection, string roleName, string dbName)
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
            Logger.WriteLine("database does not exist - skipping");
            return;
        }

        // Terminate existing connections to the database before dropping
        await connection.ExecuteAsync($"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}' AND pid <> pg_backend_pid();");
        await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {dbName}");
    }

    private async Task<bool> RoleExists(NpgsqlConnection connection, string roleName)
    {
        return await connection.QuerySingleAsync<bool>($"select exists(SELECT FROM pg_catalog.pg_roles WHERE rolname = '{roleName}');");
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

    private async Task DropRole(NpgsqlConnection connection, string roleName)
    {
        var roleExists = await RoleExists(connection, roleName);
        if (!roleExists)
        {
            Logger.WriteLine("no role found - skipping");
            return;
        }

        var roleHasDependentObjects = await RoleHasDependentObjects(connection, roleName);
        if (roleHasDependentObjects)
        {
            Logger.WriteLine("the role has dependent database(s) and will not be dropped (it will be dropped when the last database is dropped if you passed multiple databases to operate on)");
            return;
        }

        await connection.ExecuteAsync($"DROP ROLE IF EXISTS {roleName}");
    }

    private async Task<bool> RoleHasDependentObjects(NpgsqlConnection connection, string roleName)
    {
        var query = $@"SELECT EXISTS (
                        SELECT 1 FROM pg_database
                        WHERE datdba = (SELECT oid FROM pg_roles WHERE rolname = @RoleName)
                    );";

        return await connection.QueryFirstOrDefaultAsync<bool>(query, new { RoleName = roleName });
    }
}
