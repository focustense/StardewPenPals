using PenPals.Data;
using StardewValley.Quests;

namespace PenPals.UI;

internal class GiftMailLauncher(
    IViewEngine viewEngine,
    Func<ModConfig> configSelector,
    Func<ModData> dataSelector,
    Func<CustomRules> customRulesSelector,
    IMonitor monitor
)
{
    // We deliberately don't save the entire GiftMailFilters because it has live references (e.g.
    // PropertyChanged event handlers) that could cause leaks if held onto.
    private record GiftMailFilterState(
        bool IsVisible,
        bool Birthdays,
        bool Quests,
        GiftTaste MinTaste,
        string SearchText
    )
    {
        public static GiftMailFilterState FromViewModel(GiftMailViewModel vm)
        {
            return new(
                vm.FiltersVisible,
                vm.Filters.Birthdays,
                vm.Filters.Quests,
                vm.Filters.MinTaste,
                vm.Filters.SearchText
            );
        }

        public void Apply(GiftMailViewModel vm)
        {
            vm.FiltersVisible = IsVisible;
            vm.Filters.Birthdays = Birthdays;
            vm.Filters.Quests = Quests;
            vm.Filters.MinTaste = MinTaste;
            vm.Filters.SearchText = SearchText;
        }
    }

    // Filter state is preserved across successive menu opens, but (by design) not preserved across
    // game sessions, so we don't need this in save data.
    private GiftMailFilterState? previousFilterState;

    public bool Launch(Farmer who)
    {
        var viewModel = TryCreateViewModel(who);
        if (viewModel is null)
        {
            return false;
        }
        previousFilterState?.Apply(viewModel);
        var controller = viewEngine.CreateMenuControllerFromAsset(
            @"Mods/focustense.PenPals/Views/GiftMail",
            viewModel
        );
        controller.Closed += () =>
            previousFilterState = GiftMailFilterState.FromViewModel(viewModel);
        Game1.activeClickableMenu = controller.Menu;
        return true;
    }

    private static GiftTaste? GetGiftTaste(Farmer who, NPC npc, Item? item, ModConfig config)
    {
        if (
            item is null
            || config.GiftTasteVisibility == GiftTasteVisibility.None
            || (
                config.GiftTasteVisibility == GiftTasteVisibility.Known
                && !who.hasGiftTasteBeenRevealed(npc, item.ItemId)
            )
        )
        {
            return null;
        }
        int giftTaste = npc.getGiftTasteForThisItem(item);
        return (GiftTaste)giftTaste;
    }

    private static RecipientViewModel? TryCreateRecipient(
        Farmer who,
        NPC npc,
        Item item,
        ModConfig config,
        GiftMailData data,
        MailRules rules,
        IReadOnlyList<Quest> supportedQuests,
        WorldDate deliveryDate
    )
    {
        var nonGiftableReasons = rules.CheckGiftability(who, npc, item);
        if (
            (
                nonGiftableReasons
                & (NonGiftableReasons.Unmet | NonGiftableReasons.CannotReceiveGifts)
            ) != 0
        )
        {
            // For immersion, we'd rather not show unmet NPCs; and non-giftable NPCs just clutter
            // up the UI with non-actionable stuff.
            return null;
        }
        var taste = GetGiftTaste(who, npc, item, config);
        var pendingGift = data.OutgoingGifts.TryGetValue(npc.Name, out var pendingParcel)
            ? new GiftItemViewModel(pendingParcel.Gift)
            : null;
        var pendingQuest = supportedQuests
            .Select(quest => ItemQuestInfo.TryFromQuest(quest, who, npc))
            .FirstOrDefault(info => info is not null);
        return new RecipientViewModel(
            npc,
            item,
            taste,
            nonGiftableReasons,
            pendingGift,
            pendingQuest,
            deliveryDate
        );
    }

    private GiftMailViewModel? TryCreateViewModel(Farmer who)
    {
        var giftObject = who.ActiveObject;
        if (giftObject is null || !giftObject.canBeGivenAsGift())
        {
            return null;
        }
        var config = configSelector();
        if (config.RequireQuestCompletion && !who.hasSeenActiveDialogueEvent("questComplete_25"))
        {
            return null;
        }
        var customRules = customRulesSelector();
        if (customRules.Blacklist.Contains(giftObject.QualifiedItemId))
        {
            return null;
        }
        var data = dataSelector();
        var farmerId = who.UniqueMultiplayerID;
        if (!data.FarmerGiftMail.TryGetValue(farmerId, out var giftMailData))
        {
            giftMailData = new();
            data.FarmerGiftMail.Add(farmerId, giftMailData);
        }
        var rules = new MailRules(config, customRules);
        var gift = new GiftItemViewModel(giftObject);
        var deliveryDate =
            config.Scheduling == GiftShipmentScheduling.SameDay
                ? Game1.Date
                : WorldDate.ForDaysPlayed(Game1.Date.TotalDays + 1);
        var supportedQuests = config.EnableQuests
            ? who.questLog.Where(ItemQuestInfo.IsQuestSupported).ToArray()
            : [];
        var recipients = Game1
            .characterData.Keys.Select(name => Game1.getCharacterFromName(name))
            .Where(npc => npc is not null)
            .Select(npc =>
                TryCreateRecipient(
                    who,
                    npc,
                    giftObject,
                    config,
                    giftMailData,
                    rules,
                    supportedQuests,
                    deliveryDate
                )
            )
            .Where(recipient => recipient is not null)
            .Cast<RecipientViewModel>()
            .OrderByDescending(recipient => recipient.IsEnabled)
            .ThenBy(recipient => recipient.HasPendingGift)
            .ToList();
        var sender = new GiftSender(who, giftObject, config, giftMailData, monitor);
        return new(gift, recipients, sender, config);
    }
}
