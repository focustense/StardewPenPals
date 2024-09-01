using StardewUI;

namespace GiftMailer.UI;

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
    public static GiftTasteInfo Loved => loved.Value;

    /// <summary>
    /// Gift taste for a liked gift.
    /// </summary>
    public static GiftTasteInfo Liked => liked.Value;

    /// <summary>
    /// Gift taste for a disliked gift.
    /// </summary>
    public static GiftTasteInfo Disliked => disliked.Value;

    /// <summary>
    /// Gift taste for a hated gift.
    /// </summary>
    public static GiftTasteInfo Hated => hated.Value;

    private static readonly Lazy<GiftTasteInfo> loved =
        new(() => new(I18n.GiftMailMenu_Tooltip_Loved(), Sprites.EmojiGrin, Color.Cyan));
    private static readonly Lazy<GiftTasteInfo> liked =
        new(() => new(I18n.GiftMailMenu_Tooltip_Liked(), Sprites.EmojiHappy, Color.White));
    private static readonly Lazy<GiftTasteInfo> disliked =
        new(() => new(I18n.GiftMailMenu_Tooltip_Disliked(), Sprites.EmojiUnhappy, Color.Orange));
    private static readonly Lazy<GiftTasteInfo> hated =
        new(() => new(I18n.GiftMailMenu_Tooltip_Hated(), Sprites.EmojiAngry, Color.Red));

    /// <summary>
    /// Gets the info for a gift taste value as obtained from <see cref="NPC.getGiftTasteForThisItem(Item)"/>.
    /// </summary>
    /// <param name="giftTaste">The gift taste's numeric value.</param>
    /// <returns>Info about the specified taste, if available; otherwise <c>null</c>.</returns>
    public static GiftTasteInfo? ForGiftTaste(int giftTaste)
    {
        return giftTaste switch
        {
            0 => Loved,
            2 => Liked,
            4 => Disliked,
            6 => Hated,
            _ => null,
        };
    }
}
