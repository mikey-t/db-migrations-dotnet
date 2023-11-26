using System.Text.RegularExpressions;

namespace MikeyT.DbMigrations;

public static class ClassNameValidator
{
    private static readonly Regex ValidClassNameRegex = new(@"^[A-Za-z_]\w*$", RegexOptions.Compiled);

    private static readonly string[] ReservedKeywords =
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
        "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
        "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
        "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
        "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint",
        "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };


    public static bool IsValidClassName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (ValidClassNameRegex.IsMatch(name) && !IsReservedKeyword(name))
        {
            return true;
        }

        return false;
    }

    private static bool IsReservedKeyword(string name)
    {
        return Array.IndexOf(ReservedKeywords, name) != -1;
    }
}
