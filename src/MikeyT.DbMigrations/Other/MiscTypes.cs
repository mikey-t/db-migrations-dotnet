using System.Collections.Immutable;

namespace MikeyT.DbMigrations;

public static class Commands
{
    public const string Setup = "setup";
    public const string Teardown = "teardown";
    public const string List = "list";
    public const string BootstrapContext = "bootstrap";
    public static readonly ImmutableArray<string> AllCommands = ImmutableArray.Create(Setup, Teardown, List, BootstrapContext);
    public static string AllCommandsCommaSeparated { get { return string.Join(", ", AllCommands); } }
}

public record DbSetupArgs(string Command, List<DbContextInfo> DbContextInfos);

public record DbContextInfo(Type DbContextType, Type? SetupType);

public class CliParamException : Exception
{
    public CliParamException() { }
    public CliParamException(string message) : base(message) { }
    public CliParamException(string message, Exception inner) : base(message, inner) { }
}
