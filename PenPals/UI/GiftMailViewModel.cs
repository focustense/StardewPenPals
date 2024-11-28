namespace PenPals.UI;

/// <summary>
/// View model for the Gift Mail menu, displaying available recipients and choosing which should
/// receive the player's active item as a gift.
/// </summary>
public class GiftMailViewModel(GiftItemViewModel gift, IReadOnlyList<RecipientViewModel> recipients)
{
    /// <summary>
    /// Details about the item to be sent as a gift.
    /// </summary>
    public GiftItemViewModel Gift { get; } = gift;

    /// <summary>
    /// Details about each of the available recipients.
    /// </summary>
    public IReadOnlyList<RecipientViewModel> Recipients = recipients;
}
