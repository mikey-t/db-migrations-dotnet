using System.Text;

namespace MikeyT.DbMigrations;

public class TableGenerator
{
    public static string Generate(List<string> headers, List<List<string>> rows)
    {
        var sb = new StringBuilder();
        int[] columnWidths = headers.Select(header => header.Length).ToArray();

        // Column widths
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Count; i++)
            {
                columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
            }
        }

        // Header
        sb.AppendLine(string.Join(" | ", headers.Select((header, index) => header.PadRight(columnWidths[index]))));

        // Divider
        string divider = string.Join("-+-", columnWidths.Select(width => new string('-', width)));

        sb.AppendLine(divider);

        // Data rows
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(" | ", row.Select((cell, index) => cell.PadRight(columnWidths[index]))));
        }

        return sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
    }
}
