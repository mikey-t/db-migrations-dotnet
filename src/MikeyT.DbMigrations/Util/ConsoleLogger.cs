namespace MikeyT.DbMigrations;

public interface IConsoleLogger
{
    public void Write(string message);
    public void WriteLine(string message);
    public void Info(string message);
    public void Warn(string message);
    public void Error(string message);
    public void Error(Exception ex);
}

// This is pretty basic - should consider switching over to use Microsoft's ILogger (Microsoft.Extensions.Logging)
public class ConsoleLogger : IConsoleLogger
{
    public ConsoleLogger()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
    }

    public void Write(string message)
    {
        Console.Write(message);
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void Info(string message)
    {
        Console.WriteLine("ℹ️ " + message);
    }

    public void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("⚠️ " + message);
        Console.ResetColor();
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("🛑 " + message);
        Console.ResetColor();
    }

    public void Error(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("🛑 " + ex.Message);
        if (ex.InnerException is not null)
        {
            Console.WriteLine("🛑 " + ex.InnerException.Message);
        }
        Console.ResetColor();
        Console.WriteLine(ex);
    }
}
