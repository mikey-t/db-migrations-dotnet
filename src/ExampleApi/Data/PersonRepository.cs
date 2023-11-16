using Dapper;
using ExampleApi.Model;
using MikeyT.EnvironmentSettingsNS.Interface;
using Npgsql;

namespace ExampleApi.Data;

public interface IPersonRepository
{
    Task<int> AddPerson(Person person);
    Task<List<Person>> GetAll();
    Task DeleteAll();
}

public class PersonRepository : BaseRepository, IPersonRepository
{
    public PersonRepository(IConnectionStringProvider connectionStringProvider, IEnvironmentSettings environmentSettings)
        : base(connectionStringProvider, environmentSettings)
    {
    }

    public async Task<int> AddPerson(Person person)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        const string sql = "insert into person (first_name, last_name) values (@FirstName, @LastName) returning id";
        var id = await connection.ExecuteScalarAsync<int>(sql, person);
        return id;
    }

    public async Task<List<Person>> GetAll()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        return (await connection.QueryAsync<Person>("select * from person")).ToList();
    }

    public async Task DeleteAll()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.ExecuteAsync("delete from person where True");
    }
}
