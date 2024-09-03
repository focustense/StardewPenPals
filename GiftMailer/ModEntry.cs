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
        MailboxPatches.ModManifest = ModManifest;
        MailboxPatches.Monitor = Monitor;
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            transpiler: new(typeof(MailboxPatches), nameof(MailboxPatches.MailboxTranspiler))
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

    private GiftDistributor GetGiftDistributor()
    {
        var context = GetRulesContext();
        return new(context, Helper.GameContent);
    }

    private ModContext GetModContext()
    {
        return new(ModManifest, config, data, Monitor);
    }

    private RulesContext GetRulesContext()
    {
        var customRules = GetCustomRules();
        var mailRules = new MailRules(config, customRules);
        return new(ModManifest, config, data, mailRules, Monitor);
    }
}
