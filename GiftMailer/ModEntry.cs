using HarmonyLib;

namespace GiftMailer;

internal sealed class ModEntry : Mod
{
    // Initialized in Entry
    private ModConfig config = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            transpiler: new(typeof(MailboxPatches), nameof(MailboxPatches.MailboxTranspiler))
        );
    }
}
