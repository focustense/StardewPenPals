namespace GiftMailer;

/// <summary>
/// Configuration settings for GiftMailer.
/// </summary>
public class ModConfig
{
    /// <summary>
    /// Multiplier that applies to the friendship points gained from gifting.
    /// </summary>
    /// <remarks>
    /// Applies only to relationship <em>gains</em>; sending disliked/hated gifts will still incur
    /// the full penalty.
    /// </remarks>
    public float FriendshipMultiplier { get; set; } = 0.75f;

    /// <summary>
    /// When to show gift tastes in the UI.
    /// </summary>
    public GiftTasteVisibility GiftTasteVisibility { get; set; } = GiftTasteVisibility.Known;

    /// <summary>
    /// Whether to display a confirmation dialog before sending to the selected NPC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <c>true</c>, the setting also applies to gift replacements, i.e. swapping a gift that is
    /// already scheduled to send with a new one.
    /// </para>
    /// <para>
    /// If <c>false</c>, the item will be immediately removed from inventory and scheduled to send.
    /// </para>
    /// </remarks>
    public bool RequireConfirmation { get; set; } = true;

    /// <summary>
    /// Whether to require the "Making Friends" quest to have been completed before mailing gifts is
    /// allowed.
    /// </summary>
    public bool RequireQuestCompletion { get; set; } = true;
}

/// <summary>
/// Setting for when a gift taste should be visible.
/// </summary>
public enum GiftTasteVisibility
{
    /// <summary>
    /// Show the gift taste if known to the player - i.e. if it would also show up in the
    /// relationship menu for that NPC.
    /// </summary>
    Known,

    /// <summary>
    /// Always show gift tastes, regardless of whether they are known to the player.
    /// </summary>
    All,

    /// <summary>
    /// Never show gift tastes in the UI.
    /// </summary>
    None,
}
