using System.Text;

namespace GiftMailer.Commands;

internal enum BorderLine
{
    Top,
    Middle,
    Bottom,
}

internal static class StringBuilderExtensions
{
    public static void AppendColumns(
        this StringBuilder output,
        int[] columnWidths,
        params object[] values
    )
    {
        for (int i = 0; i < columnWidths.Length; i++)
        {
            var value = values.Length > i ? values[i] : "";
            var text = PadOrTruncate(value?.ToString() ?? "", columnWidths[i]);
            output.Append("│ ").Append(text).Append(' ');
        }
        output.AppendLine("│");
    }

    public static void AppendBorderLine(
        this StringBuilder output,
        int[] columnWidths,
        BorderLine type
    )
    {
        var (left, middle, right) = type switch
        {
            BorderLine.Top => ('┌', '┬', '┐'),
            BorderLine.Middle => ('├', '┼', '┤'),
            BorderLine.Bottom => ('└', '┴', '┘'),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
        for (int i = 0; i < columnWidths.Length; i++)
        {
            output.Append(i == 0 ? left : middle);
            // Add 2 to column width to account for spaces before/after text.
            output.Append(new string('─', columnWidths[i] + 2));
        }
        output.AppendLine(right.ToString());
    }

    private static string PadOrTruncate(string value, int length)
    {
        return value.Length <= length ? value.PadRight(length) : value[..(length - 1)] + "…";
    }
}
