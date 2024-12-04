using StardewValley.Quests;

namespace PenPals.Data;

/// <summary>
/// Extensions related to game quests.
/// </summary>
internal static class QuestExtensions
{
    /// <summary>
    /// Attempts to derive a unique ID for a quest.
    /// </summary>
    /// <remarks>
    /// Uses the assigned quest ID if available; otherwise, generates a synthetic ID for certain
    /// known types of unidentified quests.
    /// </remarks>
    /// <param name="quest">The quest to identify.</param>
    /// <returns>A locally (potentially time-dependent) unique ID for the quest, if available;
    /// otherwise an empty string.</returns>
    public static string SafeId(this Quest quest)
    {
        if (!string.IsNullOrEmpty(quest.id.Value))
        {
            return quest.id.Value;
        }
        if (quest.dailyQuest.Value && quest.accepted.Value)
        {
            return $"Daily_{quest.dayQuestAccepted.Value}";
        }
        return "";
    }
}
