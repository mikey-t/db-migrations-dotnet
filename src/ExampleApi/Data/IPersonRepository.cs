using ExampleApi.Model;

namespace ExampleApi;

public interface IPersonRepository
{
    Task<int> AddPerson(Person person);
    Task<List<Person>> GetAll();
    Task DeleteAll();
}
