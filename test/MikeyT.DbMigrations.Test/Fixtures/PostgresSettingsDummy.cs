namespace MikeyT.DbMigrations.Test;

public class PostgresSettingsDummy
{
    private readonly PostgresSettings _settings;

    public PostgresSettingsDummy()
    {
        _settings = new PostgresSettings(GetType());
    }

    public PostgresSettings GetSettings()
    {
        return _settings;
    }
}
