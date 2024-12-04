using System.Diagnostics.CodeAnalysis;
using PenPals.Data;
using PenPals.Logging;
using StardewValley.Quests;

namespace PenPals.Commands;

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
            var farmer = Game1.GetPlayer(playerId);
            if (farmer is null)
            {
                context.Monitor.Log($"Could not find Farmer with ID: {playerId}", LogLevel.Error);
                continue;
            }
            foreach (var (npcName, parcel) in giftData.OutgoingGifts)
            {
                var npc = Game1.getCharacterFromName(npcName);
                if (npc is null)
                {
                    context.Monitor.Log($"Could not find NPC named '{npcName}'.", LogLevel.Error);
                    continue;
                }
                var (summary, quest, points) = GetExpectedOutcome(
                    farmer,
                    npc,
                    parcel,
                    context.Rules
                );
                results.Add(new(farmer, npc, parcel.Gift, quest, summary, points));
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

    private static (string, Quest?, int) GetExpectedOutcome(
        Farmer farmer,
        NPC npc,
        Parcel parcel,
        MailRules rules
    )
    {
        var nonGiftableReasons = rules.CheckGiftability(farmer, npc, parcel.Gift, parcel.QuestId);
        if (nonGiftableReasons != 0)
        {
            return ($"Returned:{(int)nonGiftableReasons}", null, 0);
        }
        var quest = GiftDistributor.GetDeliveryQuest(farmer, parcel.QuestId);
        string summary;
        if (quest is not null)
        {
            summary = "Quest";
        }
        else
        {
            var taste = npc.getGiftTasteForThisItem(parcel.Gift);
            (summary, _) = GiftTasteBehavior.ForGiftTaste(taste);
        }
        var estimatedPoints = rules.EstimateFriendshipGain(farmer, npc, parcel.Gift, quest);
        return (summary, quest, estimatedPoints);
    }
}
