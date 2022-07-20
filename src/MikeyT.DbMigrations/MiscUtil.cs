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
    
    public static string GetEnvString(string key, bool required = true)
    {
        var val = Environment.GetEnvironmentVariable(key);
        if (required && string.IsNullOrWhiteSpace(val))
        {
            throw new Exception("Missing environment variable for key " + key);
        }

        return val ?? string.Empty;
    }
}
