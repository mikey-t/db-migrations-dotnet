namespace MikeyT.DbMigrations;

public record DbSetupArgs(string Command, List<DbContextInfo> DbContextInfos);

public record EnvSubstitution(string FromEnvKey, string ToEnvKey);

public record DbContextInfo(Type DbContextType, Type? SetupType, List<EnvSubstitution> EnvSubstitutions);

public class CliParamException : Exception
{
    public CliParamException() { }
    public CliParamException(string message) : base(message) { }
    public CliParamException(string message, Exception inner) : base(message, inner) { }
}
