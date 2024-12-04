using StardewValley.Quests;

namespace PenPals.Data;

/// <summary>
/// Helper class for distributing outgoing gifts, i.e. making NPCs actually receive them.
/// </summary>
public class GiftDistributor(RulesContext context, IGameContentHelper contentHelper)
{
    /// <summary>
    /// The context used to initialize this distributor.
    /// </summary>
    public RulesContext Context { get; } = context;

    private readonly ModConfig config = context.Config;
    private readonly ModData data = context.Data;
    private readonly IMonitor monitor = context.Monitor;
    private readonly MailRules rules = context.Rules;

    /// <summary>
    /// Attempts to retrieve the quest associated with an outgoing gift.
    /// </summary>
    /// <param name="who">The player sending the gift.</param>
    /// <param name="questId">Unique quest ID, or <c>null</c> to ignore.</param>
    /// <returns>The matching <see cref="Quest"/>, or <c>null</c> if not found.</returns>
    public static Quest? GetDeliveryQuest(Farmer who, string? questId)
    {
        return !string.IsNullOrEmpty(questId)
            ? who.questLog.FirstOrDefault(quest => quest.SafeId() == questId)
            : null;
    }

    /// <summary>
    /// Make all NPCs receive their gifts immediately.
    /// </summary>
    public IReadOnlyList<GiftResult> ReceiveAll()
    {
        var results = new List<GiftResult>();
        bool hasReturns = false;
        bool hasCompletedQuests = false;
        foreach (var (playerId, giftData) in data.FarmerGiftMail)
        {
            var farmer = Game1.GetPlayer(playerId);
            if (farmer is null)
            {
                monitor.Log($"Farmer ID {playerId} not found; skipping gifts.", LogLevel.Error);
                continue;
            }
            foreach (var (npcName, parcel) in giftData.OutgoingGifts)
            {
                var npc = Game1.getCharacterFromName(npcName);
                if (npc is null)
                {
                    monitor.Log(
                        $"NPC {npcName} not found; skipping gift from {farmer.Name}.",
                        LogLevel.Error
                    );
                    continue;
                }
                var nonGiftableReasons = rules.CheckGiftability(
                    farmer,
                    npc,
                    parcel.Gift,
                    parcel.QuestId
                );
                if (nonGiftableReasons != 0)
                {
                    results.Add(
                        new(
                            farmer,
                            npc,
                            parcel.Gift,
                            null,
                            $"Returned:{(int)nonGiftableReasons}",
                            0
                        )
                    );
                    var returnId = Guid.NewGuid().ToString();
                    giftData.ReturnedGifts.Add(
                        returnId,
                        new(npcName, parcel.Gift, Game1.Date, nonGiftableReasons)
                    );
                    farmer.mailbox.Add(GiftMailData.GetReturnMailKey(returnId));
                    hasReturns = true;
                    continue;
                }
                if (GetDeliveryQuest(farmer, parcel.QuestId) is { } quest)
                {
                    var questPoints = Context.Rules.GetQuestPoints(quest);
                    farmer.changeFriendship(questPoints, npc);
                    GamePatches.SuppressQuestSounds = true;
                    try
                    {
                        quest.questComplete();
                    }
                    finally
                    {
                        GamePatches.SuppressQuestSounds = false;
                    }
                    hasCompletedQuests = true;
                    results.Add(new(farmer, npc, parcel.Gift, quest, "Quest", questPoints));
                }
                else
                {
                    var giftTaste = npc.getGiftTasteForThisItem(parcel.Gift);
                    var (tasteName, basePoints) = GiftTasteBehavior.ForGiftTaste(giftTaste);
                    var multiplier = basePoints >= 0 ? config.FriendshipMultiplier : 1.0f;
                    var previousFriendship = farmer.tryGetFriendshipLevelForNPC(npc.Name) ?? 0;
                    LocationPatches.SuppressGiftSounds = true;
                    try
                    {
                        npc.receiveGift(
                            parcel.Gift,
                            farmer,
                            friendshipChangeMultiplier: multiplier,
                            showResponse: false
                        );
                    }
                    finally
                    {
                        LocationPatches.SuppressGiftSounds = false;
                    }
                    var nextFriendship = farmer.tryGetFriendshipLevelForNPC(npc.Name) ?? 0;
                    var pointsGained = nextFriendship - previousFriendship;
                    results.Add(new(farmer, npc, parcel.Gift, null, tasteName, pointsGained));
                }
            }
        }
        foreach (var result in results)
        {
            if (!data.FarmerGiftMail.TryGetValue(result.From.UniqueMultiplayerID, out var giftData))
            {
                continue;
            }
            giftData.OutgoingGifts.Remove(result.To.Name);
        }
        if (hasCompletedQuests)
        {
            Game1.playSound("questcomplete");
        }
        if (hasReturns)
        {
            contentHelper.InvalidateCache("Data/Mail");
        }
        return results;
    }
}
