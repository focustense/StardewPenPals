namespace GiftMailer.Data;

/// <summary>
/// Player-instanced data about sent gifts.
/// </summary>
public class GiftMailData
{
    /// <summary>
    /// Gifts scheduled to be sent but not yet "opened"; keyed by NPC's unique/internal name.
    /// </summary>
    public Dictionary<string, SObject> OutgoingGifts { get; set; } = [];
}
