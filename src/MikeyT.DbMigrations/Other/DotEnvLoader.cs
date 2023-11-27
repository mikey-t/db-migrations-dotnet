namespace MikeyT.DbMigrations;

public interface IDotEnvLoader
{
    public void EnsureLoaded();
    public void Load();
    public void Load(string filePath);
    public void SetLogEnabled(bool logEnabled);
}

public class DotEnvLoader : IDotEnvLoader
{
    private static bool _isLoaded = false;
    private static readonly object _loadLock = new object();

    private bool _logEnabled;
    private readonly IConsoleLogger _logger;
    private readonly IEnvSetter _envSetter;

    public DotEnvLoader(bool logEnabled = true)
    {
        _logger = new ConsoleLogger();
        _envSetter = new DefaultEnvSetter();
        _logEnabled = logEnabled;
    }

    public DotEnvLoader(IConsoleLogger logger, IEnvSetter envSetter, bool logEnabled = true)
    {
        _logger = logger;
        _envSetter = envSetter;
        _logEnabled = logEnabled;
    }

    public static void LoadStatic(string filePath)
    {
        new DotEnvLoader().Load(filePath);
    }

    /// <summary>
    /// Ensure the default .env has been loaded at least once.
    /// </summary>
    public static void EnsureLoadedStatic()
    {
        new DotEnvLoader().Load();
    }

    /// <summary>
    /// Ensure the default .env has been loaded at least once.
    /// </summary>
    public void EnsureLoaded()
    {
        if (!_isLoaded)
        {
            lock (_loadLock)
            {
                if (!_isLoaded)
                {
                    Load();
                    _isLoaded = true;
                }
            }
        }
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
            _logger.Info($@"DotEnvLoader is loading environment variables from ""{filePath}""");
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            if (line.Trim() == string.Empty)
            {
                continue;
            }
            var equalsIndex = line.IndexOf("=");
            if (equalsIndex <= 0 || equalsIndex >= line.Length - 1)
            {
                continue;
            }

            var firstPart = line[..equalsIndex];
            var trimmedFirstPart = firstPart.Trim();
            var secondPart = line[(equalsIndex + 1)..];

            if (string.IsNullOrWhiteSpace(trimmedFirstPart))
            {
                continue;
            }

            if (trimmedFirstPart != firstPart && _logEnabled)
            {
                _logger.Warn($@"DotEnvLoader encountered an environment variable key with leading or trailing whitespace (""{firstPart}"") and will be trimmed to ""{trimmedFirstPart}""");
            }

            _envSetter.SetEnvironmentVariable(trimmedFirstPart, secondPart);
        }
    }

    public void SetLogEnabled(bool logEnabled)
    {
        _logEnabled = logEnabled;
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
