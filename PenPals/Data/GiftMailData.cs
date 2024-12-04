namespace PenPals.Data;

/// <summary>
/// Player-instanced data about sent gifts.
/// </summary>
public class GiftMailData
{
    // This is intentionally hardcoded so that if the mod name in the ModManifest ever changes, it
    // doesn't break savegame data.
    private static readonly string ReturnMailPrefix = "focustense.PenPals:ReturnedGift_";

    /// <summary>
    /// Gets the return ID (key in <see cref="ReturnedGifts"/> dictionary) associated with a
    /// specified mail piece.
    /// </summary>
    /// <param name="mailTitle">The title/key of the mail piece in the mailbox.</param>
    /// <returns>The return ID associated with the mail piece, or <c>null</c> if it does not appear
    /// to be a gift-return mail.</returns>
    public static string? GetReturnIdFromMailKey(string mailTitle)
    {
        return mailTitle.StartsWith(ReturnMailPrefix) ? mailTitle[ReturnMailPrefix.Length..] : null;
    }

    /// <summary>
    /// Gets a mail key for a returned gift.
    /// </summary>
    /// <param name="returnId">Return ID; the key in the <see cref="ReturnedGifts"/> dictionary.</param>
    /// <returns>Mail key for the specified return.</returns>
    public static string GetReturnMailKey(string returnId)
    {
        return ReturnMailPrefix + returnId;
    }

    /// <summary>
    /// Gifts scheduled to be sent but not yet "opened"; keyed by NPC's unique/internal name.
    /// </summary>
    public Dictionary<string, Parcel> OutgoingGifts { get; set; } = [];

    /// <summary>
    /// Gifts that were returned to the player because the NPC was deemed non-giftable at the time
    /// of intended receipt.
    /// </summary>
    /// <remarks>
    /// The dictionary key is a unique ID per "instance" of returned gift, which connects the mail
    /// (received by player) to the return info.
    /// </remarks>
    public Dictionary<string, ReturnedGift> ReturnedGifts { get; set; } = [];
}

/// <summary>
/// Data about a gift that had to be returned to the player.
/// </summary>
/// <param name="NpcName">Unique name of the NPC for whom the gift was intended.</param>
/// <param name="GiftObject">The object sent as a gift.</param>
/// <param name="PickupDate">The date the mail was "picked up" - either the day it was posted, or
/// the day after, depending on the configured <see cref="ModConfig.Scheduling"/> at the time.</param>
/// <param name="Reasons">Reasons for non-giftability at the time of send-back.</param>
public record ReturnedGift(
    string NpcName,
    SObject GiftObject,
    WorldDate PickupDate,
    NonGiftableReasons Reasons
)
{
    /// <summary>
    /// Formats the return info as a mail string for viewing in the player's mailbox.
    /// </summary>
    /// <param name="includeDetails">Whether to include detailed information about the reason for
    /// the return.</param>
    public string ToMailString(bool includeDetails)
    {
        var detailsText = "";
        if (includeDetails)
        {
            var reasonText = Reasons.ToTranslatedStrings().FirstOrDefault();
            if (!string.IsNullOrEmpty(reasonText))
            {
                detailsText = I18n.Mail_GiftReturned_Details(reasonText);
            }
        }
        var npcName = Game1.getCharacterFromName(NpcName)?.displayName ?? NpcName;
        return I18n.Mail_GiftReturned_Base(PickupDate.Localize(), npcName, detailsText);
    }
}
