using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PenPals.Data;
using PenPals.UI;
using StardewValley.Menus;

namespace PenPals;

internal static class MailboxPatches
{
    // Must be set in ModEntry
    public static Func<ModConfig> ConfigSelector { get; set; } = null!;
    public static Func<CustomRules> CustomRulesSelector { get; set; } = null!;
    public static Func<ModData> DataSelector { get; set; } = null!;
    public static IManifest ModManifest { get; set; } = null!;
    public static IMonitor Monitor { get; set; } = null!;

    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Required by Harmony"
    )]
    public static IEnumerable<CodeInstruction> MailboxTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        var mailboxGetter = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.mailbox));
        var listCountGetter = AccessTools.PropertyGetter(
            typeof(ICollection<string>),
            nameof(ICollection<string>.Count)
        );
        var giftMailMenuMethod = AccessTools.Method(
            typeof(MailboxPatches),
            nameof(MaybeShowGiftMailMenu)
        );
        var activeClickableMenuSetter = AccessTools.PropertySetter(
            typeof(Game1),
            nameof(Game1.activeClickableMenu)
        );
        var addReturnedItemMethod = AccessTools.Method(
            typeof(MailboxPatches),
            nameof(MaybeAddReturnedItem)
        );
        var matcher = new CodeMatcher(instructions);
        matcher
            .MatchEndForward(new CodeMatch(OpCodes.Call, activeClickableMenuSetter))
            .Advance(1)
            .Insert(new CodeInstruction(OpCodes.Call, addReturnedItemMethod));
        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Call, mailboxGetter),
            new CodeMatch(OpCodes.Callvirt, listCountGetter),
            new CodeMatch(OpCodes.Brtrue_S)
        );
        var endLabel = matcher.Instruction.operand;
        matcher
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Call, giftMailMenuMethod),
                new CodeInstruction(OpCodes.Brtrue_S, endLabel)
            );
        return matcher.InstructionEnumeration();
    }

    private static void MaybeAddReturnedItem()
    {
        if (
            Game1.activeClickableMenu is not LetterViewerMenu menu
            || !DataSelector()
                .FarmerGiftMail.TryGetValue(Game1.player.UniqueMultiplayerID, out var giftData)
        )
        {
            return;
        }
        var returnId = GiftMailData.GetReturnIdFromMailKey(menu.mailTitle);
        if (
            string.IsNullOrEmpty(returnId)
            || !giftData.ReturnedGifts.TryGetValue(returnId, out var returnedGift)
        )
        {
            return;
        }
        // From LetterViewMenu.cs; neighbor IDs, coordinates, etc. are all hardcoded.
        menu.itemsToGrab.Add(
            new ClickableComponent(
                new Rectangle(
                    menu.xPositionOnScreen + menu.width / 2 - 48,
                    menu.yPositionOnScreen + menu.height - 32 - 96,
                    96,
                    96
                ),
                returnedGift.GiftObject
            )
            {
                myID = 104,
                leftNeighborID = 101,
                rightNeighborID = 102,
            }
        );
        menu.backButton.rightNeighborID = 104;
        menu.forwardButton.leftNeighborID = 104;
        giftData.ReturnedGifts.Remove(returnId);
    }

    private static bool MaybeShowGiftMailMenu()
    {
        var giftObject = Game1.player.ActiveObject;
        if (giftObject is null || !giftObject.canBeGivenAsGift())
        {
            return false;
        }
        var config = ConfigSelector();
        if (
            config.RequireQuestCompletion
            && !Game1.player.hasSeenActiveDialogueEvent("questComplete_25")
        )
        {
            return false;
        }
        var customRules = CustomRulesSelector();
        if (customRules.Blacklist.Contains(giftObject.QualifiedItemId))
        {
            return false;
        }
        var data = DataSelector();
        var farmerId = Game1.player.UniqueMultiplayerID;
        if (!data.FarmerGiftMail.TryGetValue(farmerId, out var giftMailData))
        {
            giftMailData = new();
            data.FarmerGiftMail.Add(farmerId, giftMailData);
        }
        var rules = new MailRules(config, customRules);
        Game1.activeClickableMenu = new GiftMailMenu(
            config,
            giftMailData,
            rules,
            Game1.player,
            Monitor
        );
        return true;
    }
}
