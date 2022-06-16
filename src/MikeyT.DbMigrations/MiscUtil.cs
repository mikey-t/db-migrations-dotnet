namespace MikeyT.DbMigrations;

public class MiscUtil
{
    public static void ConsoleError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex);
        Console.ResetColor();
    }
        
    public static void ConsoleError(string str)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(str);
        Console.ResetColor();
    }
}
