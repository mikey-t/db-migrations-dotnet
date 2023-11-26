using MikeyT.DbMigrations.Test.TestUtils;
using Xunit.Abstractions;

namespace MikeyT.DbMigrations.Test;

public class TableGeneratorTest : BaseTestWithOutput
{
    public TableGeneratorTest(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Generate_ReturnsTable()
    {
        var expectedLine1 = "Name  | Age | City";
        var expectedLine2 = "------+-----+--------------";
        var expectedLine3 = "Alice | 30  | New York";
        var expectedLine4 = "Bob   | 24  | San Francisco";

        var headers = new List<string> { "Name", "Age", "City" };
        var rows = new List<List<string>> {
            new() { "Alice", "30", "New York" },
            new() { "Bob", "24", "San Francisco" }
        };

        var tableString = TableGenerator.Generate(headers, rows);
        var actualLines = tableString
            .Split(Environment.NewLine)
            .Select(line => line.Trim())
            .ToList();

        Assert.Equal(4, actualLines.Count);
        Assert.Equal(expectedLine1, actualLines[0]);
        Assert.Equal(expectedLine2, actualLines[1]);
        Assert.Equal(expectedLine3, actualLines[2]);
        Assert.Equal(expectedLine4, actualLines[3]);
    }
}
