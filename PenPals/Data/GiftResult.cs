using StardewValley.Quests;

namespace PenPals.Data;

/// <summary>
/// The result of an NPC receiving a gift by mail.
/// </summary>
/// <param name="From">The player who sent the gift.</param>
/// <param name="To">The NPC who was intended to receive the gift.</param>
/// <param name="Gift">The gift that was sent.</param>
/// <param name="CompletedQuest">The quest, if any, that was completed by this delivery.</param>
/// <param name="Outcome">Short explanation of the outcome, either a gift taste or rejection reason.
/// Not to be displayed in UI, only logs.</param>
/// <param name="Points">Friendship points gained or lost.</param>
public record GiftResult(
    Farmer From,
    NPC To,
    SObject Gift,
    Quest? CompletedQuest,
    string Outcome,
    int Points
);
