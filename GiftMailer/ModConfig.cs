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
    public float FriendshipMultiplier { get; set; } = 0.6f;

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

    /// <summary>
    /// Configures the schedule on which gift shipments occur.
    /// </summary>
    public GiftShipmentScheduling Scheduling { get; set; } = GiftShipmentScheduling.SameDay;
}

/// <summary>
/// Allowed shipping schedules for mailed gifts.
/// </summary>
public enum GiftShipmentScheduling
{
    /// <summary>
    /// Gifts are received at the end of the same day they were shipped, after the player goes to
    /// sleep but before the day changes.
    /// </summary>
    /// <remarks>
    /// This is the "simpler" option as all the rules for mailing gifts are essentially the same as
    /// those for gifting in person, i.e. around daily/weekly limits; sending a gift on an NPC's
    /// birthday means it is actually received on their birthday, and so on.
    /// </remarks>
    SameDay,

    /// <summary>
    /// Gifts are received at the beginning of the day after they were shipped.
    /// </summary>
    /// <remarks>
    /// This is the "immersive" option that maintains consistency with how gifts are received by the
    /// player when sent by NPCs. Rules around gift limits are "deferred" - a gift can be mailed on
    /// the same day a different gift was given in person, but will count toward tomorrow's daily
    /// limit. Weekly limits are ignored if a gift is mailed on Saturday, since the limits would
    /// reset on Sunday. Birthday gifts have to be sent the day <em>before</em> an NPC's birthday in
    /// order to be exempt from limits and/or benefit from the birthday friendship bonus.
    /// </remarks>
    NextDay,
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
