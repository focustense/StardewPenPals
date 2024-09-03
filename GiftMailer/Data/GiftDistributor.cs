namespace GiftMailer.Data;

/// <summary>
/// Helper class for distributing outgoing gifts, i.e. making NPCs actually receive them.
/// </summary>
public class GiftDistributor(ModConfig config, ModData data, MailRules rules, IMonitor monitor)
{
    /// <summary>
    /// Make all NPCs receive their gifts immediately.
    /// </summary>
    public IReadOnlyList<GiftResult> ReceiveAll()
    {
        var results = new List<GiftResult>();
        foreach (var (playerId, giftData) in data.FarmerGiftMail)
        {
            var farmer = Game1.getFarmerMaybeOffline(playerId);
            if (farmer is null)
            {
                monitor.Log($"Farmer ID {playerId} not found; skipping gifts.", LogLevel.Error);
                continue;
            }
            foreach (var (npcName, giftObject) in giftData.OutgoingGifts)
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
                var nonGiftableReasons = rules.CheckGiftability(farmer, npc, giftObject);
                if (nonGiftableReasons != 0)
                {
                    results.Add(
                        new(farmer, npc, giftObject, $"Returned:{(int)nonGiftableReasons}", 0)
                    );
                    continue;
                }
                var giftTaste = npc.getGiftTasteForThisItem(giftObject);
                var (tasteName, basePoints) = GiftTasteBehavior.ForGiftTaste(giftTaste);
                var multiplier = basePoints >= 0 ? config.FriendshipMultiplier : 1.0f;
                var previousFriendship = farmer.tryGetFriendshipLevelForNPC(npc.Name) ?? 0;
                npc.receiveGift(
                    giftObject,
                    farmer,
                    friendshipChangeMultiplier: multiplier,
                    showResponse: false
                );
                var nextFriendship = farmer.tryGetFriendshipLevelForNPC(npc.Name) ?? 0;
                var pointsGained = nextFriendship - previousFriendship;
                results.Add(new(farmer, npc, giftObject, tasteName, pointsGained));
            }
        }
        foreach (var result in results)
        {
            if (!data.FarmerGiftMail.TryGetValue(result.From.UniqueMultiplayerID, out var giftData))
            {
                continue;
            }
            if (giftData.OutgoingGifts.Remove(result.To.Name) && giftData.OutgoingGifts.Count == 0)
            {
                data.FarmerGiftMail.Remove(result.From.UniqueMultiplayerID);
            }
        }
        return results;
    }
}
