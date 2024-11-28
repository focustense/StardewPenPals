namespace PenPals.UI;

/// <summary>
/// Properties of a giftable item, displayed either as the current item being sent or a previous
/// item chosen to send to an NPC.
/// </summary>
/// <param name="item">The item to be gifted.</param>
public class GiftItemViewModel(Item item)
{
    /// <summary>
    /// The image to display for this item.
    /// </summary>
    public ItemImageViewModel Image { get; } = ItemImageViewModel.ForItem(item);

    /// <summary>
    /// The complete item details.
    /// </summary>
    public Item Item { get; } = item;

    /// <summary>
    /// The item quality.
    /// </summary>
    public int Quality { get; } = item.Quality;
}
