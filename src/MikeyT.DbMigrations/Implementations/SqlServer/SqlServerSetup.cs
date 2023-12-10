using System.Data.SqlClient;
using Dapper;

namespace MikeyT.DbMigrations;

public class SqlServerSetup : DbSetup
{
    private readonly SqlServerSettings _settings;

    public SqlServerSetup() : this(new SqlServerSettings(), new ConsoleLogger()) { }

    public SqlServerSetup(SqlServerEnvKeys envKeys) : this(new SqlServerSettings(envKeys), new ConsoleLogger()) { }

    public SqlServerSetup(SqlServerSettings settings) : this(settings, new ConsoleLogger()) { }

    public SqlServerSetup(SqlServerSettings settings, IConsoleLogger logger) : base(settings, logger)
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

        Logger.WriteLine($"creating database {dbName} and user {dbUser} (server login and database user) using DbSetup connection string: {logSafeConnectionString}");

        ThrowIfBadDbNameOrUser(dbName, dbUser);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await EnsureLogin(connection, dbUser, dbPassword);
        await EnsureDatabase(connection, dbName, dbUser);
        await EnsureUser(connection, dbName, dbUser);
        await EnsureUserRoles(connection, dbName, dbUser);
    }

    public override async Task Teardown()
    {
        var connectionString = _settings.GetDbSetupConnectionString();
        var logSafeConnectionString = _settings.GetLogSafeConnectionString(connectionString);
        var dbName = _settings.GetDbName();
        var dbUser = _settings.GetDbUser();

        Logger.WriteLine($"tearing down database {dbName} and user {dbUser} using connection string: {logSafeConnectionString}");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await DropUser(connection, dbName, dbUser);
        await DropDatabase(connection, dbName);
        await DropLogin(connection, dbUser);
    }

    public override string GetDbContextBoilerplate(string dbContextName)
    {
        string boilerplate = @"using MikeyT.DbMigrations;

namespace DbMigrations;

public class PlaceholderDbContext : SqlServerMigrationsDbContext { }
";

        return boilerplate.Replace("PlaceholderDbContext", dbContextName);
    }

    private void ThrowIfBadDbNameOrUser(string dbName, string dbUser)
    {
        if (dbName.Contains('\''))
        {
            throw new Exception($"Invalid database name has single quotes: {dbName}");
        }
        if (dbUser.Contains('\''))
        {
            throw new Exception($"Invalid database user name has single quotes: {dbUser}");
        }
        if (dbName.Trim() != dbName)
        {
            throw new Exception("Database name must not have leading or trailing whitespace");
        }
        if (dbUser.Trim() != dbUser)
        {
            throw new Exception("Database user must not have leading or trailing whitespace");
        }
        if (dbUser.ToLower() == "sa")
        {
            throw new Exception($"Invalid database user - cannot use \"sa\" for application specific database");
        }
    }

    private async Task EnsureLogin(SqlConnection connection, string dbUser, string dbPassword)
    {
        if (await LoginExists(connection, dbUser))
        {
            Logger.WriteLine($"login {dbUser} already exists, skipping");
        }
        else
        {
            await CreateLogin(connection, dbUser, dbPassword);
            Logger.WriteLine($"created login {dbUser}");
        }
    }

    private async Task<bool> LoginExists(SqlConnection connection, string dbUser)
    {
        Logger.WriteLine("checking if login exists...");
        return await connection.ExecuteScalarAsync<bool>($"select count(1) from [sys].[server_principals] where [name]='{dbUser}'");
    }

    private async Task CreateLogin(SqlConnection connection, string dbUser, string password)
    {
        await connection.ExecuteAsync($"CREATE LOGIN [{dbUser}] WITH PASSWORD = '{EscapeSqlString(password)}';");
    }

    private async Task EnsureDatabase(SqlConnection connection, string dbName, string dbUser)
    {
        if (await DatabaseExists(connection, dbName))
        {
            Logger.WriteLine($"database {dbName} already exists, skipping");
        }
        else
        {
            await CreateDatabase(connection, dbName);
            Logger.WriteLine($"created database {dbName}");
        }
    }

    private async Task<bool> DatabaseExists(SqlConnection connection, string dbName)
    {
        Logger.WriteLine($"checking if database exists...");
        return await connection.ExecuteScalarAsync<bool>($"select count(1) from [sys].[databases] where [name]='{dbName}'");
    }

    private async Task CreateDatabase(SqlConnection connection, string dbName)
    {
        await connection.ExecuteAsync($"CREATE DATABASE [{dbName}]");
    }

    private async Task EnsureUser(SqlConnection connection, string dbName, string dbUser)
    {
        if (await UserExists(connection, dbName, dbUser))
        {
            Logger.WriteLine($"user {dbUser} already exists in database {dbName}, skipping");
        }
        else
        {
            await CreateDatabaseUser(connection, dbName, dbUser);
            Logger.WriteLine($"created user {dbUser} in database {dbName}");
        }
    }

    private async Task<bool> UserExists(SqlConnection connection, string dbName, string dbUser)
    {
        Logger.WriteLine("checking if database user exists...");
        return await connection.ExecuteScalarAsync<bool>($"USE [{dbName}];select count(1) from [sys].[database_principals] where [name]='{dbUser}'");
    }

    private async Task CreateDatabaseUser(SqlConnection connection, string dbName, string dbUser)
    {
        await connection.ExecuteAsync($"USE [{dbName}];CREATE USER {dbUser} FOR LOGIN [{dbUser}];");
    }

    private async Task EnsureUserRoles(SqlConnection connection, string dbName, string dbUser)
    {
        Logger.WriteLine("ensuring user belongs to roles db_datareader and db_datawriter");
        await connection.ExecuteAsync($"USE [{dbName}];ALTER ROLE db_datareader ADD MEMBER [{dbUser}];ALTER ROLE db_datawriter ADD MEMBER [{dbUser}];");
    }

    private async Task DropDatabase(SqlConnection connection, string dbName)
    {
        if (await DatabaseExists(connection, dbName))
        {
            // Drop existing connections first
            await connection.ExecuteAsync($"ALTER DATABASE {dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
            await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {dbName}");
            Logger.WriteLine($"dropped database {dbName}");
        }
        else
        {
            Logger.WriteLine($"database {dbName} does not exist - skipping");
        }
    }

    private async Task DropUser(SqlConnection connection, string dbName, string dbUser)
    {
        if (!await DatabaseExists(connection, dbName))
        {
            Logger.WriteLine("database does not exist, no need to check database for user - skipping");
            return;
        }
        if (await UserExists(connection, dbName, dbUser))
        {
            await connection.ExecuteAsync($"USE [master]; DROP USER IF EXISTS {dbUser}");
            Logger.WriteLine($"dropped user {dbUser}");
        }
        else
        {
            Logger.WriteLine($"user {dbUser} does not exist in database {dbName} - skipping");
        }
    }

    private async Task DropLogin(SqlConnection connection, string dbUser)
    {
        var loginExists = await LoginExists(connection, dbUser);
        var loginInUse = loginExists && await IsLoginUsedByAnyDatabase(connection, dbUser);
        if (loginInUse)
        {
            Logger.WriteLine($"login {dbUser} is still associated to a database user - skipping deletion (expected for multiple database shared user scenarios)");
            return;
        }
        else if (loginExists)
        {
            Logger.WriteLine($"dropping login {dbUser}...");
            await connection.ExecuteAsync($"DROP LOGIN {dbUser}");
            Logger.WriteLine($"dropped login {dbUser}");
        }
        else
        {
            Logger.WriteLine($"login {dbUser} does not exist - skipping");
        }
    }

    private async Task<bool> IsLoginUsedByAnyDatabase(SqlConnection connection, string dbUser)
    {
        Logger.WriteLine($"checking if any other database users are still associated with login {dbUser}");
        var databases = await GetDatabases(connection);
        var ignore = new List<string> { "master", "tempdb", "model", "msdb" };
        foreach (var dbName in databases)
        {
            if (ignore.Contains(dbName))
            {
                continue;
            }
            var userExistsInDb = await DoesUserExistInDatabase(connection, dbName, dbUser);
            if (userExistsInDb)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<List<string>> GetDatabases(SqlConnection connection)
    {
        return (await connection.QueryAsync<string>("USE [master];SELECT name FROM sys.databases;")).ToList();
    }

    private async Task<bool> DoesUserExistInDatabase(SqlConnection connection, string dbName, string dbUser)
    {
        return await connection.ExecuteScalarAsync<bool>($"USE [{dbName}];SELECT 1 FROM sys.database_principals WHERE name = '{dbUser}';");
    }

    private static string EscapeSqlString(string value)
    {
        return value.Replace("'", "''");
    }
}
