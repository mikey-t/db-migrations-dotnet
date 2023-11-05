using NSubstitute;
using Xunit.Abstractions;

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
        logger.Received().Error("Missing required first param must be one of the following: setup, teardown, list");
    }
}
