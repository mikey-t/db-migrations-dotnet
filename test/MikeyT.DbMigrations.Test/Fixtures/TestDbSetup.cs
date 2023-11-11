using Microsoft.EntityFrameworkCore;

namespace MikeyT.DbMigrations.Test.Fixtures;

public class TestDummyDbSetup : DbSetup
{
    public TestDummyDbSetup() : this(new TestDummyDbSettings(), new ConsoleLogger())
    {
    }

    public TestDummyDbSetup(DbSettings settings, IConsoleLogger logger) : base(settings, logger)
    {
    }

    public override string GetDbContextBoilerplate(string dbContextName)
    {
        throw new NotImplementedException();
    }

    public override Task Setup()
    {
        throw new NotImplementedException();
    }

    public override Task Teardown()
    {
        throw new NotImplementedException();
    }
}

public class TestDummyDbSettings : DbSettings
{
    public override string GetDbSetupConnectionString()
    {
        throw new NotImplementedException();
    }

    public override string GetMigrationsConnectionString()
    {
        throw new NotImplementedException();
    }
}

public class TestDummyDbContext : DbContext, IDbSetupContext<TestDummyDbSetup>
{
    public TestDummyDbSetup GetDbSetup()
    {
        throw new NotImplementedException();
    }
}
