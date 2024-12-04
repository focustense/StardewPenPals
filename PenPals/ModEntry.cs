using HarmonyLib;
using PenPals.Commands;
using PenPals.Data;
using PenPals.Integrations;
using PenPals.Integrations.Gmcm;
using PenPals.Logging;
using PenPals.UI;
using StardewModdingAPI.Events;

namespace PenPals;

internal sealed class ModEntry : Mod
{
    private string CustomRulesAssetName => $"{ModManifest.UniqueID}/Rules";

    private const string ROOT_COMMAND = "gm";

    // Initialized in Entry
    private ModConfig config = null!;
    private IViewEngine viewEngine = null!;

    // Reloaded on save
    private ModData data = new();

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.Saving += GameLoop_Saving;
        Helper.Events.Content.AssetRequested += Content_AssetRequested;

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            transpiler: new(typeof(MailboxPatches), nameof(MailboxPatches.MailboxTranspiler))
        );
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.localSound)),
            prefix: new(typeof(LocationPatches), nameof(LocationPatches.LocalSound_Prefix))
        );
        harmony.Patch(
            AccessTools.Method(
                typeof(Game1),
                nameof(Game1.playSound),
                [typeof(string), typeof(int?)]
            ),
            prefix: new(typeof(GamePatches), nameof(GamePatches.PlaySound_Prefix))
        );

        var commandHandler = new CommandHandler(Monitor, ROOT_COMMAND);
        commandHandler.AddCommand(new DryRunCommand(GetRulesContext));
        commandHandler.AddCommand(new ReceiveAllCommand(GetGiftDistributor));
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
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
        {
            e.Edit(mailData =>
            {
                var mail = mailData.AsDictionary<string, string>();
                foreach (var giftData in data.FarmerGiftMail.Values)
                {
                    foreach (var (returnId, returnedGift) in giftData.ReturnedGifts)
                    {
                        var mailKey = GiftMailData.GetReturnMailKey(returnId);
                        var descriptionText = returnedGift.ToMailString(
                            config.DetailedReturnReasons
                        );
                        mail.Data[mailKey] = descriptionText;
                    }
                }
            });
        }
    }

    private void GameLoop_DayEnding(object? sender, DayEndingEventArgs e)
    {
        if (config.Scheduling != GiftShipmentScheduling.SameDay)
        {
            return;
        }
        var distributor = GetGiftDistributor();
        var results = distributor.ReceiveAll();
        GiftLogger.LogResults(results, "End-of-Day Gift Results:", Monitor, LogLevel.Debug);
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        if (config.Scheduling != GiftShipmentScheduling.NextDay)
        {
            return;
        }
        var distributor = GetGiftDistributor();
        var results = distributor.ReceiveAll();
        GiftLogger.LogResults(results, "Start-of-Day Gift Results:", Monitor, LogLevel.Debug);
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

        viewEngine =
            Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")
            ?? throw new InvalidOperationException(
                "StardewUI Framework is not installed; most Pen Pals functions will be disabled."
            );
        viewEngine.RegisterSprites($"Mods/{ModManifest.UniqueID}/Sprites", "assets/sprites");
        viewEngine.RegisterViews($"Mods/{ModManifest.UniqueID}/Views", "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
        MailboxPatches.DataSelector = () => data;
        MailboxPatches.GiftMailLauncher = new GiftMailLauncher(
            viewEngine,
            () => config,
            () => data,
            GetCustomRules,
            Monitor
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

    private GiftDistributor GetGiftDistributor()
    {
        var context = GetRulesContext();
        return new(context, Helper.GameContent);
    }

    private RulesContext GetRulesContext()
    {
        var customRules = GetCustomRules();
        var mailRules = new MailRules(config, customRules);
        return new(ModManifest, config, data, mailRules, Monitor);
    }
}
