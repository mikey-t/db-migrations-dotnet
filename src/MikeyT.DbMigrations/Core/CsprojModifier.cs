using System.Xml.Linq;

namespace MikeyT.DbMigrations;

public class CsprojModifier
{
    private readonly ICsprojAccessor _csprojAccessor;

    public CsprojModifier() : this(new CsprojAccessor()) { }

    public CsprojModifier(ICsprojAccessor csprojAccessor)
    {
        _csprojAccessor = csprojAccessor;
    }

    // Returns true if change was made, false otherwise
    public bool EnsureFolderInclude(string csprojPath, string folderIncludePath)
    {
        var doc = _csprojAccessor.LoadProject(csprojPath);

        if (doc.Root == null)
        {
            throw new Exception("Csproj file document root is null");
        }

        var hasFolderElement = doc.Descendants()
            .Any(x => x.Name.LocalName == "Folder" && x.Attribute("Include")?.Value == folderIncludePath);

        if (hasFolderElement)
        {
            return false;
        }

        var itemGroup = new XElement("ItemGroup");
        itemGroup.Add(new XElement("Folder", new XAttribute("Include", folderIncludePath)));
        doc.Root.Add(itemGroup);

        _csprojAccessor.SaveProject(csprojPath, doc);

        return true;
    }
}
