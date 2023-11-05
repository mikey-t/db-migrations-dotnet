namespace MikeyT.DbMigrations;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DbSetupClassAttribute : Attribute
{
    public Type SetupClass { get; }

    public DbSetupClassAttribute(Type setupClass)
    {
        if (!typeof(DbSetup).IsAssignableFrom(setupClass))
        {
            throw new ArgumentException($"{setupClass} must be a type of DbSetupBase");
        }

        SetupClass = setupClass;
    }
}
