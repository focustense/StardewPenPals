using PenPals.Data;

namespace PenPals.UI;

internal class GiftMailLauncher(
    IViewEngine viewEngine,
    Func<ModConfig> configSelector,
    Func<ModData> dataSelector,
    Func<CustomRules> customRulesSelector,
    IMonitor monitor
)
{
    public bool Launch(Farmer who)
    {
        var viewModel = TryCreateViewModel(who);
        if (viewModel is null)
        {
            return false;
        }
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(
            @"Mods/focustense.PenPals/Views/GiftMail",
            viewModel
        );
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
        MailRules rules
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
        var pendingGift = data.OutgoingGifts.TryGetValue(npc.Name, out var pendingItem)
            ? new GiftItemViewModel(pendingItem)
            : null;
        return new RecipientViewModel(npc, item, taste, nonGiftableReasons, pendingGift);
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
        var recipients = Game1
            .characterData.Keys.Select(name => Game1.getCharacterFromName(name))
            .Where(npc => npc is not null)
            .Select(npc => TryCreateRecipient(who, npc, giftObject, config, giftMailData, rules))
            .Where(recipient => recipient is not null)
            .Cast<RecipientViewModel>()
            .OrderByDescending(recipient => recipient.IsEnabled)
            .ToList();
        var sender = new GiftSender(who, giftObject, config, giftMailData, monitor);
        return new(gift, recipients, sender);
    }
}
