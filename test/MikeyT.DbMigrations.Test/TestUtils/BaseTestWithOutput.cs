using System.Text;
using Xunit.Abstractions;

namespace MikeyT.DbMigrations.Test.TestUtils;

/// <summary>
/// <para>Note that output encoding can't be changed and doesn't support UTF-8, so emoji's won't be output, for example.</para>
/// 
/// <para>Also note that you can't redirect all console output using <c>Console.SetOut(new XunitConsoleRedirect(output));</c>
/// because it will cause this error whenever <c>Console.WriteLine</c> is called: "There is no currently active test.".</para>
/// </summary>
public abstract class BaseTestWithOutput
{
    protected ITestOutputHelper Output { get; }

    protected BaseTestWithOutput(ITestOutputHelper output)
    {
        Output = output;
    }

    private class XunitConsoleRedirect : TextWriter
    {
        private readonly ITestOutputHelper _output;

        public XunitConsoleRedirect(ITestOutputHelper output)
        {
            _output = output;
        }

        public override void WriteLine(string? message)
        {
            _output.WriteLine(message ?? string.Empty);
        }

        public override void WriteLine(string? format, params object?[]? args)
        {
            _output.WriteLine(format ?? "", args ?? Array.Empty<object>());
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
