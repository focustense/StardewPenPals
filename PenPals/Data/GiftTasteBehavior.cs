namespace PenPals.Data;

/// <summary>
/// Behavioral information about a gift taste value.
/// </summary>
/// <param name="InternalName">Name of the value for displaying in logs, etc. Do not use in UI.</param>
/// <param name="BasePoints">Friendship points gained (or lost), before scaling is applied.</param>
public record GiftTasteBehavior(string InternalName, int BasePoints)
{
    private static readonly GiftTasteBehavior special = new("Special", 250); // Stardrop Tea
    private static readonly GiftTasteBehavior love = new("Love", 80);
    private static readonly GiftTasteBehavior like = new("Like", 45);
    private static readonly GiftTasteBehavior neutral = new("Neutral", 20);
    private static readonly GiftTasteBehavior dislike = new("Dislike", -20);
    private static readonly GiftTasteBehavior hate = new("Hate", -40);

    /// <summary>
    /// Gets the info for a gift taste value as obtained from <see cref="NPC.getGiftTasteForThisItem(Item)"/>.
    /// </summary>
    /// <param name="giftTaste">The gift taste's numeric value.</param>
    /// <returns>The <see cref="GiftTasteBehavior"/> for the specified taste value.</returns>
    public static GiftTasteBehavior ForGiftTaste(int giftTaste)
    {
        return giftTaste switch
        {
            0 => love,
            2 => like,
            4 => dislike,
            6 => hate,
            7 => special,
            _ => neutral,
        };
    }
}
