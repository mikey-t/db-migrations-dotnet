using System.Xml.Linq;

namespace MikeyT.DbMigrations;

public interface ICsprojAccessor
{
    XDocument LoadProject(string path);
    void SaveProject(string path, XDocument document);
}

public class CsprojAccessor : ICsprojAccessor
{
    public virtual XDocument LoadProject(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception($"Csproj file does not exist: {path}");
        }

        try
        {
            return XDocument.Load(path);
        }
        catch (Exception ex)
        {
            throw new Exception("Error loading csproj file", ex);
        }
    }

    public virtual void SaveProject(string path, XDocument document)
    {
        document.Save(path);
    }
}
