using StardewValley.Quests;

namespace PenPals.Data;

/// <summary>
/// Details about a quest involving the delivery of an item to an NPC.
/// </summary>
/// <remarks>
/// <para>
/// This can be related to an <see cref="ItemDeliveryQuest"/> or a different type of quest with
/// similar mechanics, such as <see cref="FishingQuest"/>.
/// </para>
/// <para>
/// The type is meant to be converted via duck typing to Stardew UI's <c>TooltipData</c>, so the
/// names of the fields are important and should not be changed.
/// </para>
/// </remarks>
/// <param name="Id">Unique ID for the corresponding <see cref="Quest"/>.</param>
/// <param name="Title">The formatted title, used in tooltips.</param>
/// <param name="Text">The formatted text, which is an abbreviated version of the quest description
/// and shown in the tooltip body.</param>
/// <param name="RequiredItemId">ID of the item that must be delivered to complete the quest.</param>
/// <param name="RequiredItemAmount">Number of items that must be delivered.</param>
/// <param name="CurrencyAmount">Amount of money to display, e.g. as a reward.</param>
public record ItemQuestInfo(
    string Id,
    string Title,
    string Text,
    string RequiredItemId,
    int RequiredItemAmount,
    int? CurrencyAmount
)
{
    /// <summary>
    /// Checks if a quest is supported by the mod.
    /// </summary>
    /// <remarks>
    /// This is used as an initial filter to limit the quests passed into <see cref="TryFromQuest"/>
    /// in order to speed up menu creation.
    /// </remarks>
    /// <param name="quest">The quest to check.</param>
    /// <returns><c>true</c> if the <paramref name="quest"/> <b>might</b> yield a valid result when
    /// provided to <see cref="TryFromQuest"/>; <c>false</c> if it can never be satisfied.</returns>
    public static bool IsQuestSupported(Quest quest)
    {
        return !quest.completed.Value && quest is FishingQuest or ItemDeliveryQuest;
    }

    /// <summary>
    /// Attempts to resolve the delivery info for a quest. Requires a compatible quest type.
    /// </summary>
    /// <param name="quest">The active quest.</param>
    /// <param name="who">The current player; used for inventory checks.</param>
    /// <param name="npc">The NPC who would receive the gift.</param>
    /// <returns>The delivery details for the specified <paramref name="quest"/>, or <c>null</c> if
    /// the quest is not a compatible type or cannot yet be completed.</returns>
    public static ItemQuestInfo? TryFromQuest(Quest quest, Farmer who, NPC npc)
    {
        string questId = quest.SafeId();
        if (string.IsNullOrEmpty(questId))
        {
            return null;
        }
        var (itemId, itemCount) = quest switch
        {
            FishingQuest fq when IsCompletable(fq, npc) => (fq.ItemId.Value, 1),
            ItemDeliveryQuest dq when IsCompletable(dq, npc) => (dq.ItemId.Value, dq.number.Value),
            _ => ("", 0),
        };
        if (string.IsNullOrEmpty(itemId) || itemCount == 0)
        {
            return null;
        }
        var heldCount = who.Items.CountId(itemId);
        var description = I18n.GiftMailMenu_Tooltip_Quest_Description(
            quest.currentObjective,
            heldCount
        );
        int? moneyAmount = quest.moneyReward.Value > 0 ? quest.moneyReward.Value : null;
        return new(questId, quest.questTitle, description, itemId, itemCount, moneyAmount);
    }

    private static bool IsCompletable(FishingQuest quest, NPC npc)
    {
        if (quest.numberFished.Value < quest.numberToFish.Value)
        {
            return false;
        }
        // Willy fallback is hardcoded in FishingQuest logic.
        string targetNpcName = !string.IsNullOrEmpty(quest.target.Value)
            ? quest.target.Value
            : "Willy";
        return npc.Name == targetNpcName;
    }

    private static bool IsCompletable(ItemDeliveryQuest quest, NPC npc)
    {
        return npc.Name == quest.target.Value;
    }
}
