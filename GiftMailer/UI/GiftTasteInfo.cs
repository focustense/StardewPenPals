using StardewUI;

namespace PenPals.UI;

/// <summary>
/// UI-optimized information about an NPC's taste for a particular gift.
/// </summary>
/// <remarks>
/// Neutral gift tastes are generally not shown, and have no predefined data.
/// </remarks>
/// <param name="Description">Friendly description to show in the tooltip.</param>
/// <param name="Sprite">Emoji sprite representing the NPC's reaction.</param>
/// <param name="Tint">Tint color to apply to the <paramref name="Sprite"/> for improved visibility.</param>
public record GiftTasteInfo(string Description, Sprite Sprite, Color Tint)
{
    /// <summary>
    /// Gift taste for a loved gift.
    /// </summary>
    public static GiftTasteInfo Love => love.Value;

    /// <summary>
    /// Gift taste for a liked gift.
    /// </summary>
    public static GiftTasteInfo Like => like.Value;

    /// <summary>
    /// Gift taste for a disliked gift.
    /// </summary>
    public static GiftTasteInfo Dislike => dislike.Value;

    /// <summary>
    /// Gift taste for a hated gift.
    /// </summary>
    public static GiftTasteInfo Hate => hate.Value;

    private static readonly Lazy<GiftTasteInfo> love =
        new(() => new(I18n.GiftMailMenu_Tooltip_Love(), Sprites.EmojiGrin, Color.Cyan));
    private static readonly Lazy<GiftTasteInfo> like =
        new(() => new(I18n.GiftMailMenu_Tooltip_Like(), Sprites.EmojiHappy, Color.White));
    private static readonly Lazy<GiftTasteInfo> dislike =
        new(() => new(I18n.GiftMailMenu_Tooltip_Dislike(), Sprites.EmojiUnhappy, Color.Orange));
    private static readonly Lazy<GiftTasteInfo> hate =
        new(() => new(I18n.GiftMailMenu_Tooltip_Hate(), Sprites.EmojiAngry, Color.Red));

    /// <summary>
    /// Gets the info for a gift taste value as obtained from <see cref="NPC.getGiftTasteForThisItem(Item)"/>.
    /// </summary>
    /// <param name="giftTaste">The gift taste's numeric value.</param>
    /// <returns>Info about the specified taste, if available; otherwise <c>null</c>.</returns>
    public static GiftTasteInfo? ForGiftTaste(int giftTaste)
    {
        return giftTaste switch
        {
            0 => Love,
            2 => Like,
            4 => Dislike,
            6 => Hate,
            _ => null,
        };
    }
}
