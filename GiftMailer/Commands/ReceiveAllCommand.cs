using System.Diagnostics.CodeAnalysis;
using System.Text;
using GiftMailer.Data;

namespace GiftMailer.Commands;

internal record ReceiveAllArgs();

internal class ReceiveAllCommand(
    Func<ModConfig> configSelector,
    Func<ModData> dataSelector,
    Func<CustomRules> customRulesSelector,
    IMonitor monitor
) : ICommand<ReceiveAllArgs>
{
    public string Name => "receiveall";

    public string Description =>
        "Make all NPCs receive their gifts immediately, ignoring mail schedules.";

    private static readonly int[] COLUMN_WIDTHS = [12, 12, 20, 14, 5];

    public void Execute(ReceiveAllArgs args)
    {
        var config = configSelector();
        var data = dataSelector();
        var rules = new MailRules(config, customRulesSelector());
        var distributor = new GiftDistributor(config, data, rules, monitor);
        var results = distributor.ReceiveAll();
        PrintResults(results);
    }

    public bool TryParseArgs(
        string[] args,
        [MaybeNullWhen(false)] out ReceiveAllArgs parsedArgs,
        [MaybeNullWhen(true)] out string error
    )
    {
        parsedArgs = new();
        error = null;
        return true;
    }

    private static char GetQualityChar(int quality)
    {
        return quality switch
        {
            1 => 'S',
            2 => 'G',
            4 => 'I',
            _ => '?',
        };
    }

    private void PrintResults(IEnumerable<GiftResult> results)
    {
        var output = new StringBuilder();
        output.AppendLine("Results of Gift Mail dry-run:");
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Top);
        output.AppendColumns(COLUMN_WIDTHS, "From", "To", "Gift", "Reaction", "Pts");
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Middle);
        foreach (var result in results)
        {
            var giftName = result.Gift.Name;
            if (result.Gift.Quality > 0)
            {
                giftName = "(" + GetQualityChar(result.Gift.Quality) + ") " + giftName;
            }
            output.AppendColumns(
                COLUMN_WIDTHS,
                result.From.Name,
                result.To.Name,
                giftName,
                result.Outcome,
                result.Points
            );
        }
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Bottom);
        monitor.Log(output.ToString(), LogLevel.Info);
    }
}
