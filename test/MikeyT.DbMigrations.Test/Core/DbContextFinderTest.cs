using MikeyT.DbMigrations.Test.Fixtures;
using MikeyT.DbMigrations.Test.TestUtils;
using Xunit.Abstractions;

namespace MikeyT.DbMigrations.Test;

public class DbContextFinderTest : BaseTestWithOutput
{
    public DbContextFinderTest(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void GetAllDbContextInfos_ThreeContextsOneWithoutDbSetup_ReturnsAllButOneHasNullDbSetupProp()
    {
        var results = new DbContextFinder(true).GetAllDbContextInfos();
        var sortedResults = results.OrderBy(r => r.DbContextType.Name).ToList();

        Assert.Collection(sortedResults,
            item =>
            {
                Assert.Equal(typeof(NoSetupTypeDbContext), item.DbContextType);
                Assert.Null(item.SetupType);
            },
            item =>
            {
                Assert.Equal(typeof(TestDummyDbContext), item.DbContextType);
                Assert.Equal(typeof(TestDummyDbSetup), item.SetupType);
            },
            item =>
            {
                Assert.Equal(typeof(TestPostgresDbContext), item.DbContextType);
                Assert.Equal(typeof(PostgresSetup), item.SetupType);
            }
        );
    }
}
