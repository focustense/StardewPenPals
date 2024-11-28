using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PenPals.Data;

namespace PenPals.UI;

/// <summary>
/// Properties of a gift recipient to display in the gifting menu.
/// </summary>
/// <param name="npc">The NPC to receive the gift.</param>
/// <param name="item">The item to be gifted.</param>
/// <param name="taste">The <paramref name="npc"/>'s taste for the specified
/// <paramref name="item"/>, if the taste should be shown.</param>
/// <param name="nonGiftableReasons">Bitfield including all reasons (if any) for why the specified
/// <paramref name="item"/> cannot currently be gifted to the <paramref name="npc"/>.</param>
/// <param name="pendingGift">The previous gift already scheduled (but not yet sent/received) for
/// this recipient. Can be swapped or cancelled.</param>
public class RecipientViewModel(
    NPC npc,
    Item item,
    GiftTaste? taste,
    NonGiftableReasons nonGiftableReasons,
    GiftItemViewModel? pendingGift
)
{
    /// <summary>
    /// Color tint to apply to the portrait frame.
    /// </summary>
    public Color BackgroundTint { get; } =
        pendingGift is not null ? new(0.8f, 1.0f, 0.8f) : Color.White;

    /// <summary>
    /// Whether the recipient has a <see cref="PendingGift"/>.
    /// </summary>
    public bool HasPendingGift => PendingGift is not null;

    /// <summary>
    /// Whether the recipient can be selected for receiving the gift.
    /// </summary>
    public bool IsEnabled { get; } = nonGiftableReasons == 0;

    /// <summary>
    /// Name of the recipient (NPC).
    /// </summary>
    public string Name { get; } = npc.displayName;

    /// <summary>
    /// The original NPC data.
    /// </summary>
    public NPC Npc { get; } = npc;

    /// <summary>
    /// The previous gift already scheduled (but not yet sent/received) for this recipient. Can be
    /// swapped or cancelled.
    /// </summary>
    public GiftItemViewModel? PendingGift { get; } = pendingGift;

    /// <summary>
    /// Sprite data for the NPC's portrait.
    /// </summary>
    public Tuple<Texture2D, Rectangle> Portrait { get; } =
        Tuple.Create(npc.Portrait, Game1.getSourceRectForStandardTileSheet(npc.Portrait, 0));

    /// <summary>
    /// Color tint to apply to the portrait.
    /// </summary>
    public Color PortraitTint { get; } =
        nonGiftableReasons != 0 ? new(0.15f, 0.15f, 0.15f, 0.5f)
        : pendingGift is not null ? new(Color.DimGray, 0.35f)
        : Color.White;

    /// <summary>
    /// The recipient's expected reaction (gift taste) for the selected item.
    /// </summary>
    public GiftTaste? Reaction { get; } = taste;

    /// <summary>
    /// Color tint to apply to the reaction sprite, if displaying a <see cref="Reaction"/>.
    /// </summary>
    public Color ReactionTint { get; } =
        nonGiftableReasons != 0
            ? new(Color.DarkGray, 0.5f)
            : GetTasteTint(taste) * (pendingGift is not null ? 0.7f : 1f);

    /// <summary>
    /// Localized tooltip including the <see cref="Name"/>, <see cref="Reaction"/> and item name.
    /// </summary>
    /// <seealso cref="GiftTasteExtensions.GetDescription(GiftTaste, string)"/>
    public string? TooltipText { get; } =
        FormatTooltip(npc.displayName, item.DisplayName, taste, nonGiftableReasons);

    private static string FormatTooltip(
        string npcName,
        string itemName,
        GiftTaste? taste,
        NonGiftableReasons nonGiftableReasons
    )
    {
        var sb = new StringBuilder(npcName);
        if (taste.HasValue)
        {
            string tasteDescription = taste.Value.GetDescription(itemName);
            if (!string.IsNullOrEmpty(tasteDescription))
            {
                sb.AppendLine().Append('(').Append(tasteDescription).Append(')');
            }
        }
        if (nonGiftableReasons != 0)
        {
            sb.AppendLine().AppendLine().Append(I18n.GiftMailMenu_Tooltip_NonGiftable());
            foreach (var reasonText in nonGiftableReasons.ToTranslatedStrings())
            {
                sb.AppendLine().Append("* ").Append(reasonText);
            }
        }
        return sb.ToString();
    }

    private static Color GetTasteTint(GiftTaste? taste)
    {
        return taste switch
        {
            GiftTaste.Love => Color.Cyan,
            GiftTaste.Dislike => Color.Orange,
            GiftTaste.Hate => Color.Red,
            _ => Color.White,
        };
    }
}
