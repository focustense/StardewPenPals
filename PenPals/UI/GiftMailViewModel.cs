using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace PenPals.UI;

/// <summary>
/// View model for the Gift Mail menu, displaying available recipients and choosing which should
/// receive the player's active item as a gift.
/// </summary>
/// <param name="gift">Details about the gift being sent.</param>
/// <param name="recipients">Details about all possible gift recipients (i.e. NPCs).</param>
/// <param name="sender">The sender instance to use for the selected recipient.</param>
public partial class GiftMailViewModel(
    GiftItemViewModel gift,
    IReadOnlyList<RecipientViewModel> recipients,
    GiftSender sender
) : INotifyPropertyChanged
{
    /// <summary>
    /// Color tint for the filter enable/disable button.
    /// </summary>
    public Color FilterButtonTint => FiltersVisible ? Color.Gray : Color.White;

    /// <summary>
    /// The current filters.
    /// </summary>
    public GiftMailFilters Filters { get; } = new();

    /// <summary>
    /// Details about the item to be sent as a gift.
    /// </summary>
    public GiftItemViewModel Gift { get; } = gift;

    /// <summary>
    /// Details about each of the available recipients.
    /// </summary>
    public IEnumerable<RecipientViewModel> Recipients => GetFilteredRecipients();

    /// <summary>
    /// Whether to show the filter panel.
    /// </summary>
    [Notify]
    private bool filtersVisible;

    private bool isWatchingFilters;

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
        var questToResolve =
            recipient.PendingQuest is not null
            && recipient.PendingQuest.RequiredItemId == Gift.Item.QualifiedItemId
            && Gift.Item.Stack >= recipient.PendingQuest.RequiredItemAmount
                ? recipient.PendingQuest
                : null;
        sender.Send(recipient.Npc, questToResolve);
        return true;
    }

    /// <summary>
    /// Toggles the visibility of the filter bar.
    /// </summary>
    /// <returns>Always <c>true</c> to stop event bubbling.</returns>
    public bool ToggleFilters()
    {
        Game1.playSound("smallSelect");
        FiltersVisible = !FiltersVisible;
        return true;
    }

    private void Filters_PropertyChanged(object? o, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(new(nameof(Recipients)));
    }

    private IEnumerable<RecipientViewModel> GetFilteredRecipients()
    {
        // Silly hack to watch filters here, since we can't initialize the event handler in the
        // primary constructor. About the same amount of code as converting to regular ctor.
        WatchFilters();
        IEnumerable<RecipientViewModel> result = recipients;
        if (Filters.Birthdays || Filters.Quests || Filters.MinTaste != GiftTaste.Neutral)
        {
            result = result.Where(r =>
                (Filters.Birthdays && r.HasBirthday)
                || (Filters.Quests && r.HasPendingQuest)
                || (
                    Filters.MinTaste != GiftTaste.Neutral
                    && r.Reaction?.IsAtLeast(Filters.MinTaste) == true
                )
            );
        }
        if (!string.IsNullOrWhiteSpace(Filters.SearchText))
        {
            result = result.Where(r =>
                r.Name.Contains(Filters.SearchText, StringComparison.CurrentCultureIgnoreCase)
            );
        }
        return result;
    }

    private void WatchFilters()
    {
        if (isWatchingFilters)
        {
            return;
        }
        Filters.PropertyChanged += Filters_PropertyChanged;
        isWatchingFilters = true;
    }
}
