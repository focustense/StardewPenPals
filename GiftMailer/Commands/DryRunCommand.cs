using System.Diagnostics.CodeAnalysis;
using GiftMailer.Data;
using GiftMailer.Logging;

namespace GiftMailer.Commands;

internal record DryRunArgs();

internal class DryRunCommand(Func<RulesContext> contextSelector) : ICommand<DryRunArgs>
{
    public string Name => "dryrun";

    public string Description =>
        "Display the expected results of the next gift mailing, but don't actually send the gifts.";

    public void Execute(DryRunArgs args)
    {
        var context = contextSelector();
        var results = new List<GiftResult>();
        foreach (var (playerId, giftData) in context.Data.FarmerGiftMail)
        {
            var farmer = Game1.getFarmerMaybeOffline(playerId);
            if (farmer is null)
            {
                context.Monitor.Log($"Could not find Farmer with ID: {playerId}", LogLevel.Error);
                continue;
            }
            foreach (var (npcName, giftObject) in giftData.OutgoingGifts)
            {
                var npc = Game1.getCharacterFromName(npcName);
                if (npc is null)
                {
                    context.Monitor.Log($"Could not find NPC named '{npcName}'.", LogLevel.Error);
                    continue;
                }
                var (tasteName, points) = GetExpectedOutcome(
                    farmer,
                    npc,
                    giftObject,
                    context.Rules
                );
                results.Add(new(farmer, npc, giftObject, tasteName, points));
            }
        }
        GiftLogger.LogResults(results, "Results of Gift Mail dry-run:", context.Monitor);
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
        Farmer farmer,
        NPC npc,
        SObject giftObject,
        MailRules rules
    )
    {
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
}
