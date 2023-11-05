using Microsoft.EntityFrameworkCore.Migrations;

namespace MikeyT.DbMigrations;

public class MigrationScriptRunner
{
    private static ISqlPlaceholderReplacer _replacer = new DefaultSqlPlaceholderReplacer();

    public static void RunScript(MigrationBuilder migrationBuilder, string relativePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"Scripts/{relativePath}");
        var sql = File.ReadAllText(path);
        sql = _replacer.GetSqlWithPlaceholderReplacements(sql);
        migrationBuilder.Sql(sql);
    }

    public static void SetSqlPlaceholderReplacer(ISqlPlaceholderReplacer replacer)
    {
        _replacer = replacer;
    }
}

public interface ISqlPlaceholderReplacer
{
    string GetSqlWithPlaceholderReplacements(string sql);
}

public class DefaultSqlPlaceholderReplacer : ISqlPlaceholderReplacer
{
    public string GetSqlWithPlaceholderReplacements(string sql)
    {
        return sql.Replace(":DB_USER", Environment.GetEnvironmentVariable("DB_USER"));
    }
}
