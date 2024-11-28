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
}
