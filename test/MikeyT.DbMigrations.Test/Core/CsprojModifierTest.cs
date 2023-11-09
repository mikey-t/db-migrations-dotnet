using System.Xml.Linq;
using MikeyT.DbMigrations.Test.TestUtils;
using NSubstitute;
using Xunit.Abstractions;

namespace MikeyT.DbMigrations.Test;

public class CsprojModifierTest : BaseTestWithOutput
{
    private const string FOLDER_PATH = "Migrations/MainDbContextMigrations";
    private const string CSPROJ_PATH_WITH_FOLDER = "./Fixtures/CsprojWithFolder.txt";
    private const string CSPROJ_PATH_WITHOUT_FOLDER = "./Fixtures/CsprojWithoutFolder.txt";
    private readonly XDocument _originalDocWithFolder;
    private readonly XDocument _originalDocWithoutFolder;
    private readonly string _originalWithoutFolder;

    public CsprojModifierTest(ITestOutputHelper output) : base(output)
    {
        _originalDocWithFolder = XDocument.Load(CSPROJ_PATH_WITH_FOLDER);
        _originalDocWithoutFolder = XDocument.Load(CSPROJ_PATH_WITHOUT_FOLDER);
        _originalWithoutFolder = _originalDocWithoutFolder.ToString();
    }

    [Fact]
    public void EnsureFolderInclude_CsprojAlreadyHasFolder_CsprojUnchanged()
    {
        var csprojAccessor = Substitute.For<ICsprojAccessor>();
        csprojAccessor.LoadProject(CSPROJ_PATH_WITH_FOLDER).Returns(_originalDocWithFolder);

        var csprojWasModified = new CsprojModifier(csprojAccessor).EnsureFolderInclude(CSPROJ_PATH_WITH_FOLDER, FOLDER_PATH);

        Assert.False(csprojWasModified);
        csprojAccessor.DidNotReceiveWithAnyArgs().SaveProject(default!, default!);
    }

    [Fact]
    public void EnsureFolderInclude_CsprojDoesNotHaveFolder_CsprojChanged()
    {
        var csprojAccessor = Substitute.For<CsprojAccessor>();
        csprojAccessor.LoadProject(CSPROJ_PATH_WITHOUT_FOLDER).Returns(_originalDocWithoutFolder);
        XDocument? capturedDoc = null;
        csprojAccessor.SaveProject(Arg.Any<string>(), Arg.Do<XDocument>(doc => capturedDoc = doc));

        var csprojWasModified = new CsprojModifier(csprojAccessor).EnsureFolderInclude(CSPROJ_PATH_WITHOUT_FOLDER, FOLDER_PATH);

        Assert.True(csprojWasModified);
        Assert.NotNull(capturedDoc);
        var capturedDocString = capturedDoc.ToString();
        Assert.NotEqual(_originalWithoutFolder, capturedDocString);
        bool containsFolderLine = capturedDocString.Split('\n').Any(line => line.Contains("Folder") && line.Contains("Include") && line.Contains($@"""{FOLDER_PATH}"""));
        Assert.True(containsFolderLine);
    }
}
