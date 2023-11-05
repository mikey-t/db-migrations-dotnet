using MikeyT.DbMigrations.Test.TestUtils;
using Xunit.Abstractions;

namespace MikeyT.DbMigrations.Test;

public class PostgresSettingsTest : BaseTestWithOutput
{
    public PostgresSettingsTest(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Ctor_SettingsPopulatedFromEnv()
    {
        var settings = new PostgresSettingsDummy().GetSettings();
        Assert.Equal("dbmigrationsexample", settings.DbName);
        Assert.Equal("PasswordForUnitTest1234!", settings.DbPassword);
    }
    
    [Fact]
    [Trait("Category", "only")]
    public void Ctor_PopulatesEnvSubstitutions()
    {
        var settings = new PostgresSettingsDummyWithSubstitutions().GetSettings();
        Assert.Equal("test_dbmigrationsexample", settings.DbName);
        Assert.Equal("PasswordForUnitTest1234!_TEST", settings.DbPassword);
    }
}
