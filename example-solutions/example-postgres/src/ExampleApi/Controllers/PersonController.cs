using System.Net;
using ExampleApi.Data;
using ExampleApi.Logic;
using ExampleApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers;

[ApiController]
[Route("api/person")]
public class PersonController : ControllerBase
{
    private static readonly Random _random = new();

    private readonly ILogger<PersonController> _logger;
    private readonly IPersonRepository _repo;
    private readonly INameLogic _nameLogic;

    public PersonController(ILogger<PersonController> logger, IPersonRepository personRepository, INameLogic nameLogic)
    {
        _logger = logger;
        _repo = personRepository;
        _nameLogic = nameLogic;
    }

    [HttpGet("random")]
    [ProducesResponseType(typeof(Person), (int)HttpStatusCode.OK)]
    public IActionResult GetRandom()
    {
        var nameLogic = new NameLogic();

        var person = new Person
        {
            Id = _random.Next(0, 999999),
            FirstName = nameLogic.GetRandomFirstName(),
            LastName = nameLogic.GetRandomLastName()
        };

        return Ok(person);
    }
    
    [HttpPut("random/{num:int}")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<IActionResult> SaveRandomPeople(int num)
    {
        if (num is < 1 or > 10)
        {
            return BadRequest("Num must be between 1 and 10");
        }

        for (var i = 0; i < num; i++)
        {
            await _repo.AddPerson(new Person
            {
                FirstName = _nameLogic.GetRandomFirstName(),
                LastName = _nameLogic.GetRandomLastName()
            });
        }
        
        return Ok();
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<Person>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAll()
    {
        var people = await _repo.GetAll();
        return Ok(people);
    }

    [HttpDelete("all")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> DeleteAll()
    {
        await _repo.DeleteAll();
        return NoContent();
    }
}
