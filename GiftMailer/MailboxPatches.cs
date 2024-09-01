using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using GiftMailer.Data;
using GiftMailer.UI;
using HarmonyLib;

namespace GiftMailer;

internal static class MailboxPatches
{
    // Must be set in ModEntry
    public static Func<ModConfig> ConfigSelector { get; set; } = null!;
    public static Func<ModData> DataSelector { get; set; } = null!;
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
        var matcher = new CodeMatcher(instructions).MatchEndForward(
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

    private static bool MaybeShowGiftMailMenu()
    {
        if (Game1.player.ActiveObject is null)
        {
            return false;
        }
        var config = ConfigSelector();
        var data = DataSelector();
        var farmerId = Game1.player.UniqueMultiplayerID;
        if (!data.FarmerGiftMail.TryGetValue(farmerId, out var giftMailData))
        {
            giftMailData = new();
            data.FarmerGiftMail.Add(farmerId, giftMailData);
        }
        Game1.activeClickableMenu = new GiftMailMenu(config, giftMailData, Game1.player, Monitor);
        return true;
    }
}
