using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace PenPals.UI;

/// <summary>
/// View model for the Gift Mail filters, limiting which recipients are displayed.
/// </summary>
public partial class GiftMailFilters : INotifyPropertyChanged
{
    /// <summary>
    /// Tint color for the birthday filter button, depending on whether the filter is active.
    /// </summary>
    public Color BirthdayButtonTint => Birthdays ? Color.White : FilterInactiveColor;

    /// <summary>
    /// Tint color for the quest filter button, depending on whether the filter is active.
    /// </summary>
    public Color QuestButtonTint => Quests ? Color.White : FilterInactiveColor;

    /// <summary>
    /// Tint color for the taste filter button, depending on the current minimum.
    /// </summary>
    public Color TasteButtonTint =>
        MinTaste != GiftTaste.Neutral ? Color.White : FilterInactiveColor;

    private static readonly Color FilterInactiveColor = Color.White * 0.5f;

    /// <summary>
    /// Whether to show only NPCs with birthdays on the delivery date.
    /// </summary>
    [Notify]
    private bool birthdays;

    /// <summary>
    /// Whether to show only NPCs with active delivery quests.
    /// </summary>
    [Notify]
    private bool quests;

    /// <summary>
    /// Textual filter; if non-empty, only NPCs whose names contain the text will be displayed.
    /// </summary>
    [Notify]
    private string searchText = "";

    /// <summary>
    /// Exclude NPCs with lower gift tastes.
    /// </summary>
    /// <remarks>
    /// In this context, <see cref="GiftTaste.Neutral"/> is used to disable the filter.
    /// </remarks>
    [Notify]
    private GiftTaste minTaste = GiftTaste.Neutral;

    /// <summary>
    /// Clears all filters.
    /// </summary>
    /// <returns>Always <c>true</c> to stop event bubbling.</returns>
    public bool Clear()
    {
        Game1.playSound("smallSelect");
        Birthdays = false;
        Quests = false;
        MinTaste = GiftTaste.Neutral;
        SearchText = "";
        return true;
    }

    /// <summary>
    /// Toggles the state of the <see cref="Birthdays"/> filter.
    /// </summary>
    /// <returns>Always <c>true</c> to stop event bubbling.</returns>
    public bool ToggleBirthdays()
    {
        Game1.playSound("smallSelect");
        Birthdays = !Birthdays;
        return true;
    }

    /// <summary>
    /// Toggles the state of the <see cref="Quests"/> filter.
    /// </summary>
    /// <returns>Always <c>true</c> to stop event bubbling.</returns>
    public bool ToggleQuests()
    {
        Game1.playSound("smallSelect");
        Quests = !Quests;
        return true;
    }

    /// <summary>
    /// Toggles the <see cref="MinTaste"/> between no filter, liked gifts or loved gifts.
    /// </summary>
    /// <returns>Always <c>true</c> to stop event bubbling.</returns>
    public bool ToggleTaste()
    {
        Game1.playSound("smallSelect");
        MinTaste = MinTaste switch
        {
            GiftTaste.Neutral => GiftTaste.Like,
            GiftTaste.Like => GiftTaste.Love,
            _ => GiftTaste.Neutral,
        };
        return true;
    }
}
