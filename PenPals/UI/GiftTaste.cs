namespace PenPals.UI;

/// <summary>
/// Enum definition for the gift taste value.
/// </summary>
/// <remarks>
/// Values for each enum field are set to the equivalent value in Stardew Valley for convenient
/// conversion.
/// </remarks>
public enum GiftTaste
{
    Love = 0,
    Like = 2,
    Dislike = 4,
    Hate = 6,
    Neutral = 8,
}

/// <summary>
/// Extensions for the <see cref="GiftTaste"/> enum.
/// </summary>
public static class GiftTasteExtensions
{
    /// <summary>
    /// Gets a localized description for a combination of gift taste and item name.
    /// </summary>
    /// <param name="taste">The gift taste.</param>
    /// <param name="itemName">Name of the item being given as a gift.</param>
    /// <returns>Localized gift taste description, e.g. "Hates Carp".</returns>
    public static string GetDescription(this GiftTaste taste, string itemName)
    {
        return taste switch
        {
            GiftTaste.Love => I18n.GiftMailMenu_Tooltip_Love(itemName),
            GiftTaste.Like => I18n.GiftMailMenu_Tooltip_Like(itemName),
            GiftTaste.Dislike => I18n.GiftMailMenu_Tooltip_Dislike(itemName),
            GiftTaste.Hate => I18n.GiftMailMenu_Tooltip_Hate(itemName),
            _ => "",
        };
    }

    /// <summary>
    /// Checks if the specified <see cref="GiftTaste"/> is at least some minimum value.
    /// </summary>
    /// <param name="taste">The taste to check.</param>
    /// <param name="min">The minimum taste required.</param>
    /// <returns><c>true</c> if the <paramref name="taste"/> is at least as "positive" as the
    /// specified <paramref name="min"/>, otherwise <c>false</c>.</returns>
    public static bool IsAtLeast(this GiftTaste taste, GiftTaste min)
    {
        return GetRank(taste) >= GetRank(min);
    }

    private static int GetRank(GiftTaste taste)
    {
        return taste switch
        {
            GiftTaste.Hate => -2,
            GiftTaste.Dislike => -1,
            GiftTaste.Like => 1,
            GiftTaste.Love => 2,
            _ => 0,
        };
    }
}
