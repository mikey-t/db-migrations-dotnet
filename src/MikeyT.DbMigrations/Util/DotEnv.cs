namespace MikeyT.DbMigrations;

public interface IDotEnv
{
    public void Load();
    public void Load(string filePath);
    public void SetLogEnabled(bool logEnabled);
}

public class DotEnv : IDotEnv
{
    private bool _logEnabled;
    private readonly IConsoleLogger _logger;
    private readonly IEnvSetter _envSetter;

    public DotEnv(bool logEnabled = true)
    {
        _logger = new ConsoleLogger();
        _envSetter = new DefaultEnvSetter();
        _logEnabled = logEnabled;
    }

    public DotEnv(IConsoleLogger logger, IEnvSetter envSetter, bool logEnabled = true)
    {
        _logger = logger;
        _envSetter = envSetter;
        _logEnabled = logEnabled;
    }

    public static void LoadStatic()
    {
        new DotEnv().Load();
    }

    public static void LoadStatic(string filePath)
    {
        new DotEnv().Load(filePath);
    }

    public void Load()
    {
        Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
    }

    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            if (_logEnabled)
            {
                _logger.Warn($@"No env file found at ""{filePath}""");
            }
            return;
        }

        if (_logEnabled)
        {
            _logger.Info($@"DotEnv is loading environment variables from ""{filePath}""");
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            if (line.Trim() == string.Empty)
            {
                continue;
            }
            var equalsIndex = line.IndexOf("=");
            if (equalsIndex < 1 || equalsIndex == line.Length - 1)
            {
                continue;
            }

            var firstPart = line.Substring(0, equalsIndex);
            var secondPart = line.Substring(equalsIndex + 1);

            _envSetter.SetEnvironmentVariable(firstPart, secondPart);
        }
    }

    public void SetLogEnabled(bool logEnabled)
    {
        this._logEnabled = logEnabled;
    }
}

public interface IEnvSetter
{
    public void SetEnvironmentVariable(string key, string? value);
}

public class DefaultEnvSetter : IEnvSetter
{
    public void SetEnvironmentVariable(string key, string? value)
    {
        Environment.SetEnvironmentVariable(key, value);
    }
}
