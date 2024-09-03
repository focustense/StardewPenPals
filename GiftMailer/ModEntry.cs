using GiftMailer.Commands;
using GiftMailer.Data;
using GiftMailer.Integrations.Gmcm;
using HarmonyLib;
using StardewGiftMailer.Integrations;
using StardewModdingAPI.Events;
using StardewUI;

namespace GiftMailer;

internal sealed class ModEntry : Mod
{
    private string CustomRulesAssetName => $"{ModManifest.UniqueID}/Rules";

    private const string ROOT_COMMAND = "gm";

    // Initialized in Entry
    private ModConfig config = null!;

    // Reloaded on save
    private ModData data = new();

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        Logger.Monitor = Monitor;

        Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.Saving += GameLoop_Saving;
        Helper.Events.Content.AssetRequested += Content_AssetRequested;

        MailboxPatches.ConfigSelector = () => config;
        MailboxPatches.CustomRulesSelector = GetCustomRules;
        MailboxPatches.DataSelector = () => data;
        MailboxPatches.Monitor = Monitor;
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            transpiler: new(typeof(MailboxPatches), nameof(MailboxPatches.MailboxTranspiler))
        );

        var commandHandler = new CommandHandler(Monitor, ROOT_COMMAND);
        commandHandler.AddCommand(
            new DryRunCommand(() => config, () => data, GetCustomRules, Monitor)
        );
        commandHandler.AddCommand(
            new ReceiveAllCommand(() => config, () => data, GetCustomRules, Monitor)
        );
        Helper.ConsoleCommands.Add(
            ROOT_COMMAND,
            $"Run commands associated with {ModManifest.Name}. Type '{ROOT_COMMAND} help' for options.",
            (_, args) => commandHandler.RunCommand(args)
        );
    }

    private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(CustomRulesAssetName))
        {
            e.LoadFromModFile<CustomRules>("assets/rules.json", AssetLoadPriority.Low);
        }
    }

    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Apis.LoadAll(Helper.ModRegistry);
        ConfigMenu.Register(
            ModManifest,
            () => config,
            reset: () => config = new(),
            save: () => Helper.WriteConfig(config)
        );
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        data = Helper.Data.ReadSaveData<ModData>(ModManifest.UniqueID) ?? new();
    }

    private void GameLoop_Saving(object? sender, SavingEventArgs e)
    {
        Helper.Data.WriteSaveData(ModManifest.UniqueID, data);
    }

    private CustomRules GetCustomRules()
    {
        return Helper.GameContent.Load<CustomRules>(CustomRulesAssetName);
    }
}
