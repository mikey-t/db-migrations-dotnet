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
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null, false)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    public void IsValidClassName_ReturnsExpectedResult(string className, bool expectedResult)
    {
        var result = ClassNameValidator.IsValidClassName(className);
        Assert.Equal(expectedResult, result);
    }
}
