namespace PenPals.UI;

/// <summary>
/// View model for the Gift Mail menu, displaying available recipients and choosing which should
/// receive the player's active item as a gift.
/// </summary>
/// <param name="gift">Details about the gift being sent.</param>
/// <param name="recipients">Details about all possible gift recipients (i.e. NPCs).</param>
/// <param name="sender">The sender instance to use for the selected recipient.</param>
public class GiftMailViewModel(
    GiftItemViewModel gift,
    IReadOnlyList<RecipientViewModel> recipients,
    GiftSender sender
)
{
    /// <summary>
    /// Details about the item to be sent as a gift.
    /// </summary>
    public GiftItemViewModel Gift { get; } = gift;

    /// <summary>
    /// Details about each of the available recipients.
    /// </summary>
    public IReadOnlyList<RecipientViewModel> Recipients = recipients;

    /// <summary>
    /// Attempts to send the current gift item to the selected recipient.
    /// </summary>
    /// <param name="recipient">The chosen recipient.</param>
    /// <returns><c>true</c> if the <see cref="Gift"/> was or will be sent; <c>false</c> if the recipient is not
    /// allowed.</returns>
    public bool SelectRecipient(RecipientViewModel recipient)
    {
        if (!recipient.IsEnabled)
        {
            return false;
        }
        sender.Send(recipient.Npc);
        return true;
    }
}
