namespace MikeyT.DbMigrations.Test;

public class ClassNameValidatorTest
{
    [Theory]
    [InlineData("MyClass", true)]
    [InlineData("My_Class", true)]
    [InlineData("MyClass1", true)]
    [InlineData("_MyClass", true)]
    [InlineData("1MyClass", false)]
    [InlineData("My Class", false)]
    [InlineData("class", false)]
    [InlineData("MyClass@", false)]
    [InlineData("My*Class", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidClassName_ReturnsExpectedResult(string className, bool expectedResult)
    {
        var result = ClassNameValidator.IsValidClassName(className);
        Assert.Equal(expectedResult, result);
    }
}
