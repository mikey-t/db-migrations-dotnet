using MikeyT.EnvironmentSettingsNS.Interface;

namespace ExampleApi.Data;

public interface IConnectionStringProvider
{
    string GetConnectionString(string dbName, string host, string port, string userId, string password);
    string GetConnectionString(string dbName);
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IEnvironmentSettings _envSettings;
    private readonly DbType _dbType;

    public ConnectionStringProvider(IEnvironmentSettings envSettings, DbType dbType)
    {
        _envSettings = envSettings;
        _dbType = dbType;
    }

    public string GetConnectionString(string dbName, string host, string port, string userId, string password)
    {
        return _dbType switch
        {
            DbType.Postgres => GetConnectionStringForPostgres(dbName, host, port, userId, password),
            DbType.SqlServer => GetConnectionStringForSqlServer(dbName, host, port, userId, password),
            _ => throw new Exception("DbType not implemented: " + Enum.GetName(_dbType))
        };
    }

    public string GetConnectionString(string dbName)
    {
        var host = _envSettings.GetString(GlobalSettings.DB_HOST);
        var port = _envSettings.GetString(GlobalSettings.DB_PORT);
        var user = _envSettings.GetString(GlobalSettings.DB_USER);
        var pass = _envSettings.GetString(GlobalSettings.DB_PASSWORD);
        return GetConnectionString(dbName, host, port, user, pass);
    }

    public string GetConnectionStringForPostgres(string dbName, string host, string port, string userId, string password)
    {
        return $"Host={host};Port={port};Database={dbName};User Id={userId};Password={password};Include Error Detail=true;";
    }

    public string GetConnectionStringForPostgres(string dbName)
    {
        var host = _envSettings.GetString(GlobalSettings.DB_HOST);
        var port = _envSettings.GetString(GlobalSettings.DB_PORT);
        var user = _envSettings.GetString(GlobalSettings.DB_USER);
        var pass = _envSettings.GetString(GlobalSettings.DB_PASSWORD);
        return GetConnectionStringForPostgres(dbName, host, port, user, pass);
    }

    public string GetConnectionStringForSqlServer(string dbName, string host, string port, string userId, string password)
    {
        return $"Server={host},{port};Database={dbName};User Id={userId};Password={password};TrustServerCertificate=True;";
    }

    public string GetConnectionStringForSqlServer(string dbName)
    {
        var host = _envSettings.GetString(GlobalSettings.DB_HOST);
        var port = _envSettings.GetString(GlobalSettings.DB_PORT);
        var user = _envSettings.GetString(GlobalSettings.DB_USER);
        var pass = _envSettings.GetString(GlobalSettings.DB_PASSWORD);
        return GetConnectionStringForSqlServer(dbName, host, port, user, pass);
    }
}
