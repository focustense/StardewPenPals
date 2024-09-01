using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace GiftMailer;

internal static class MailboxPatches
{
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
        return false;
    }
}
