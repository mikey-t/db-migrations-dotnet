using System.Data.SqlClient;
using Dapper;
using ExampleApi.Model;
using MikeyT.EnvironmentSettingsNS.Interface;

namespace ExampleApi.Data;

public class PersonRepositorySqlServer : BaseRepository, IPersonRepository
{
    public PersonRepositorySqlServer(IConnectionStringProvider connectionStringProvider, IEnvironmentSettings environmentSettings)
        : base(connectionStringProvider, environmentSettings)
    {
    }

    public async Task<int> AddPerson(Person person)
    {
        await using var connection = new SqlConnection(ConnectionString);
        const string sql = "INSERT INTO person (first_name, last_name) OUTPUT INSERTED.id VALUES (@FirstName, @LastName)";
        var id = await connection.ExecuteScalarAsync<int>(sql, person);
        return id;
    }

    public async Task<List<Person>> GetAll()
    {
        await using var connection = new SqlConnection(ConnectionString);
        return (await connection.QueryAsync<Person>("select * from person")).ToList();
    }

    public async Task DeleteAll()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.ExecuteAsync("delete from person");
    }
}
