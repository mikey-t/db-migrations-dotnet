using System.Reflection;
using MikeyT.DbMigrations.Test.Fixtures;

namespace MikeyT.DbMigrations.Test;

public class TypeHelperTest
{
    public const string TestAssemblyName = "MikeyT.DbMigrations.Test";

    [Theory]
    [InlineData("PostgresSetup", true)]
    [InlineData(" PostgresSetup ", true)]
    [InlineData("MikeyT.DbMigrations.PostgresSetup", true)]
    [InlineData("NonExistentShouldReturnFalse", false)]
    public void DoesClassExist_ReturnsExpectedResult(string className, bool expectedResult)
    {
        var dbMigrationsLibAssembly = Assembly.Load(GlobalConstants.AssemblyName);
        Assert.True(dbMigrationsLibAssembly != null);
        var actualResult = TypeHelper.DoesClassExist(className, dbMigrationsLibAssembly);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    public void DoesClassExist_MissingClassName_Throws(string className)
    {
        var dbMigrationsLibAssembly = Assembly.Load(GlobalConstants.AssemblyName);
        Assert.True(dbMigrationsLibAssembly != null);
        var exception = Assert.Throws<ArgumentException>(() => TypeHelper.DoesClassExist(className, dbMigrationsLibAssembly));
        Assert.Contains("missing required param", exception.Message);
    }

    [Fact]
    public void DoesClassExist_NullAssembly_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TypeHelper.DoesClassExist("anything", null!));
    }

    [Theory]
    [InlineData("PostgresSetup", true)]
    [InlineData(" PostgresSetup ", true)]
    [InlineData("MikeyT.DbMigrations.PostgresSetup", true)]
    [InlineData("NonExistentShouldReturnFalse", false)]
    public void DoesExactlyOneClassExist_ReturnsExpectedResult(string className, bool expectedResult)
    {
        var dbMigrationsLibAssembly = Assembly.Load(GlobalConstants.AssemblyName);
        Assert.True(dbMigrationsLibAssembly != null);
        var actualResult = TypeHelper.DoesExactlyOneClassExist(className, dbMigrationsLibAssembly);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void DoesExactlyOneClassExist_MissingClassName_Throws(string className)
    {
        var dbMigrationsLibAssembly = Assembly.Load(GlobalConstants.AssemblyName);
        Assert.True(dbMigrationsLibAssembly != null);
        var exception = Assert.Throws<ArgumentException>(() => TypeHelper.DoesExactlyOneClassExist(className, dbMigrationsLibAssembly));
        Assert.Contains("missing required param", exception.Message);
    }

    [Fact]
    public void DoesExactlyOneClassExist_NullAssembly_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TypeHelper.DoesExactlyOneClassExist("anything", null!));
    }

    [Fact]
    public void DoesExactlyOneClassExist_TwoExist_ReturnsFalse()
    {
        var assembly = Assembly.Load(TestAssemblyName);
        var exists = TypeHelper.DoesClassExist("ClassNameExistence", assembly);
        Assert.True(exists);
        var existsOnce = TypeHelper.DoesExactlyOneClassExist("ClassNameExistence", assembly);
        Assert.False(existsOnce);
    }

    [Fact]
    public void DoesExactlyOneClassExist_TwoExistButNamespaceIncludedInClassName_ReturnsTrue()
    {
        var assembly = Assembly.Load(TestAssemblyName);
        var exists = TypeHelper.DoesClassExist("ClassNameExistence", assembly);
        Assert.True(exists);
        var existsOnce = TypeHelper.DoesExactlyOneClassExist("ClassNameExistenceNamespaceTwo.ClassNameExistence", assembly);
        Assert.True(existsOnce);
    }

    [Fact]
    public void GetTypeIfExactlyOne_TwoExist_ReturnsNull()
    {
        var assembly = Assembly.Load(TestAssemblyName);
        var exists = TypeHelper.DoesClassExist("ClassNameExistence", assembly);
        Assert.True(exists);
        var existsOnce = TypeHelper.GetTypeIfExactlyOne("ClassNameExistence", assembly);
        Assert.Null(existsOnce);
    }

    [Fact]
    public void GetTypeIfExactlyOne_TwoExistButNamespaceIncludedInClassName_ReturnsType()
    {
        var assembly = Assembly.Load(TestAssemblyName);
        var exists = TypeHelper.DoesClassExist("ClassNameExistence", assembly);
        Assert.True(exists);
        var existsOnce = TypeHelper.GetTypeIfExactlyOne("ClassNameExistenceNamespaceTwo.ClassNameExistence", assembly);
        Assert.NotNull(existsOnce);
    }

    [Fact]
    public void TypeImplementsInterface_MainDbContextHasGenericSetupContextInterface_ReturnsTrue()
    {
        var hasInterface = TypeHelper.TypeImplementsInterface(typeof(TestPostgresDbContext), typeof(IDbSetupContext<>));
        Assert.True(hasInterface);
    }

    [Fact]
    public void TypeImplementsInterface_ClassHasInterfaceWithGenerics_ReturnsTrue()
    {
        var hasInterface = TypeHelper.TypeImplementsInterface(typeof(MyGenericClass), typeof(IMyGenericInterface<>));
        Assert.True(hasInterface);
    }

    [Fact]
    public void TypeImplementsInterface_ClassHasInterfaceWithoutGenerics_ReturnsTrue()
    {
        var hasInterface = TypeHelper.TypeImplementsInterface(typeof(MyClass), typeof(IMyNonGenericInterface));
        Assert.True(hasInterface);
    }

    [Fact]
    public void TypeImplementsInterface_ClassDoesNotHaveInterface_ReturnsFalse()
    {
        var hasInterface = TypeHelper.TypeImplementsInterface(typeof(MyClassWithNoInterface), typeof(IMyNonGenericInterface));
        Assert.False(hasInterface);
    }

    [Fact]
    public void GetGenericInterfaceType_MainDbContextHasGenericSetupContextInterface_ReturnsType()
    {
        var interfaceType = TypeHelper.GetGenericInterfaceType(typeof(TestPostgresDbContext), typeof(IDbSetupContext<>));
        Assert.NotNull(interfaceType);
        Assert.Equal(typeof(PostgresSetup), interfaceType);
    }

    [Fact]
    public void GetGenericInterfaceType_TypeHasGenericInterface_ReturnsType()
    {
        var interfaceType = TypeHelper.GetGenericInterfaceType(typeof(MyGenericClass), typeof(IMyGenericInterface<>));
        Assert.NotNull(interfaceType);
        Assert.Equal(typeof(string), interfaceType);
    }

    [Fact]
    public void GetGenericInterfaceType_NonGenericInterface_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => TypeHelper.GetGenericInterfaceType(typeof(MyClass), typeof(IMyNonGenericInterface)));
        Assert.Contains("The provided interface type must be a generic type definition", exception.Message);
    }

    [Fact]
    public void GetGenericInterfaceType_TypeDoesNotHaveInterface_ReturnsNull()
    {
        var interfaceType = TypeHelper.GetGenericInterfaceType(typeof(MyClassWithNoInterface), typeof(IMyGenericInterface<>));
        Assert.Null(interfaceType);
    }
}
