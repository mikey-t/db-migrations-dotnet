using MikeyT.DbMigrations.Test.TestUtils;
using Xunit.Abstractions;

namespace MikeyT.DbMigrations.Test;

public class DbContextFinderTest : BaseTestWithOutput
{
    public DbContextFinderTest(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void GetAllDbContextInfos_TwoContextsOneWithAttribute_ReturnsBothButOneHasNullDbSetupProp()
    {
        var results = new DbContextFinder(true).GetAllDbContextInfos();

        Assert.Collection(
            results,
            item =>
            {
                Assert.Equal(typeof(MainDbContext), item.DbContextType);
                Assert.Equal(typeof(PostgresSetup), item.SetupType);
            },
            item =>
            {
                Assert.Equal(typeof(NoSetupTypeDbContext), item.DbContextType);
                Assert.Null(item.SetupType);
            }
        );
    }
}
