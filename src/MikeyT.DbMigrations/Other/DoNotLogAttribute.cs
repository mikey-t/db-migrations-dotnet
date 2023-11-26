namespace MikeyT.DbMigrations;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class DoNotLogAttribute : Attribute
{
    public DoNotLogAttribute() { }
}
