namespace MikeyT.DbMigrations;

public abstract class DbSetup
{
    private readonly DbSettings _settings;
    public DbSettings Settings
    {
        get
        {
            return _settings;
        }
    }

    private readonly IConsoleLogger _logger;
    protected IConsoleLogger Logger
    {
        get
        {
            return _logger;
        }
    }

    public DbSetup(DbSettings settings, IConsoleLogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public abstract Task Setup();
    public abstract Task Teardown();
    public abstract string GetDbContextBoilerplate(string dbContextName);

    public void LoadSettings()
    {
        _settings.Load();
    }
}
