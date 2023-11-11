using NSubstitute;
using Xunit.Abstractions;
using MikeyT.DbMigrations.Test.Fixtures;

namespace MikeyT.DbMigrations.Test;

public class DbSetupCliTest
{
    private readonly ITestOutputHelper output;

    public DbSetupCliTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task NoParams_Returns1AndLogsError()
    {
        var logger = Substitute.For<IConsoleLogger>();
        var result = await new DbSetupCli(logger, new DbContextFinder()).Run(Array.Empty<string>());
        Assert.Equal(1, result);
        logger.Received().Error("Missing required first param must be one of the following: setup, teardown, list, bootstrap");
    }
    
    // [Fact]
    // [Trait("Category", "only")]
    // public async Task SetupCommand_MainPassed_DoesStuff()
    // {
    //     var logger = Substitute.For<IConsoleLogger>();
    //     var dbContextFinder = Substitute.For<IDbContextFinder>();
    //     var dbContextInfo = new DbContextInfo(typeof(TestPostgresDbContext), typeof(PostgresSetup));
    //     dbContextFinder.GetAllDbContextInfos().Returns(new List<DbContextInfo> { dbContextInfo });
    //     var result = await new DbSetupCli(logger, dbContextFinder, true).Run(new[] { "setup", "main" });
    //     Assert.Equal(0, result);
    //     // logger.Received().Info("Setting up MainDbContext");
    // }
}
