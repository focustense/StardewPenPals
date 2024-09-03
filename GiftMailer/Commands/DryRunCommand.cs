using System.Diagnostics.CodeAnalysis;
using System.Text;
using GiftMailer.Data;
using StardewValley.GameData.Characters;

namespace GiftMailer.Commands;

internal record DryRunArgs();

internal class DryRunCommand(
    Func<ModConfig> configSelector,
    Func<ModData> dataSelector,
    Func<CustomRules> customRulesSelector,
    IMonitor monitor
) : ICommand<DryRunArgs>
{
    public string Name => "dryrun";

    public string Description =>
        "Display the expected results of the next gift mailing, but don't actually send the gifts.";

    private static readonly int[] COLUMN_WIDTHS = [12, 12, 20, 14, 5];

    public void Execute(DryRunArgs args)
    {
        var config = configSelector();
        var data = dataSelector();
        var rules = new MailRules(config, customRulesSelector());
        var output = new StringBuilder();
        output.AppendLine("Results of Gift Mail dry-run:");
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Top);
        output.AppendColumns(COLUMN_WIDTHS, "From", "To", "Gift", "Reaction", "Pts");
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Middle);
        foreach (var (playerId, giftData) in data.FarmerGiftMail)
        {
            var farmer = Game1.getFarmerMaybeOffline(playerId);
            var farmerName = farmer?.Name ?? "???";
            foreach (var (npcName, giftObject) in giftData.OutgoingGifts)
            {
                var npc = Game1.getCharacterFromName(npcName);
                var (tasteName, points) = GetExpectedOutcome(farmer, npc, giftObject, rules);
                var giftName = giftObject.Name;
                if (giftObject.Quality > 0)
                {
                    giftName = "(" + GetQualityChar(giftObject.Quality) + ") " + giftName;
                }
                output.AppendColumns(
                    COLUMN_WIDTHS,
                    farmerName,
                    npcName,
                    giftName,
                    tasteName,
                    points
                );
            }
        }
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Bottom);
        monitor.Log(output.ToString(), LogLevel.Info);
    }

    public bool TryParseArgs(
        string[] args,
        [MaybeNullWhen(false)] out DryRunArgs parsedArgs,
        [MaybeNullWhen(true)] out string error
    )
    {
        parsedArgs = new();
        error = null;
        return true;
    }

    private static (string, int) GetExpectedOutcome(
        Farmer? farmer,
        NPC? npc,
        SObject giftObject,
        MailRules rules
    )
    {
        if (farmer is null)
        {
            return ("FARMER MISSING", 0);
        }
        else if (npc is null)
        {
            return ("NPC MISSING", 0);
        }
        var nonGiftableReasons = rules.CheckGiftability(farmer, npc, giftObject);
        if (nonGiftableReasons != 0)
        {
            return ($"Returned:{(int)nonGiftableReasons}", 0);
        }
        var taste = npc.getGiftTasteForThisItem(giftObject);
        var (tasteName, _) = GiftTasteBehavior.ForGiftTaste(taste);
        var estimatedPoints = rules.EstimateFriendshipGain(farmer, npc, giftObject);
        return (tasteName, estimatedPoints);
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
}
