namespace ExampleApi.Logic;

public interface INameLogic
{
    string GetRandomFirstName();
    string GetRandomLastName();
}

public class NameLogic : INameLogic
{
    private static readonly Random _random = new();
    private const string FirstNamesFilename = "FirstNames.txt";
    private const string LastNamesFilename = "LastNames.txt";

    public List<string> AllFirstNames = new List<string>();
    public List<string> AllLastNames = new List<string>();
    
    public NameLogic()
    {
        PopulateNamesFromFiles();
    }

    private void PopulateNamesFromFiles()
    {
        AllFirstNames = File.ReadAllLines(FirstNamesFilename).ToList();
        AllLastNames = File.ReadAllLines(LastNamesFilename).ToList();
    }

    public string GetRandomFirstName()
    {
        return AllFirstNames[_random.Next(0, AllFirstNames.Count)];
    }

    public string GetRandomLastName()
    {
        return AllLastNames[_random.Next(0, AllLastNames.Count)];
    }
}

