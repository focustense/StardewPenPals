using GenericModConfigMenu;
using StardewGiftMailer.Integrations;

namespace GiftMailer.Integrations.Gmcm;

internal static class ConfigMenu
{
    public static void Register(IManifest mod, Func<ModConfig> config, Action reset, Action save)
    {
        if (Apis.Gmcm is not IGenericModConfigMenuApi gmcm)
        {
            return;
        }
        gmcm.Register(mod, reset, save);
        gmcm.AddParagraph(mod, I18n.Gmcm_Title);
        gmcm.AddSectionTitle(mod, I18n.Gmcm_Ui_Heading);
        gmcm.AddEnum(
            mod,
            getValue: () => config().GiftTasteVisibility,
            setValue: value => config().GiftTasteVisibility = value,
            name: I18n.Gmcm_Ui_GiftTasteVisibility_Name,
            tooltip: I18n.Gmcm_Ui_GiftTasteVisibility_Tooltip
        );
        gmcm.AddBoolOption(
            mod,
            getValue: () => config().RequireConfirmation,
            setValue: value => config().RequireConfirmation = value,
            name: I18n.Gmcm_Ui_RequireConfirmation_Name,
            tooltip: I18n.Gmcm_Ui_RequireConfirmation_Tooltip
        );
        gmcm.AddSectionTitle(mod, I18n.Gmcm_Gameplay_Heading);
        gmcm.AddBoolOption(
            mod,
            getValue: () => config().RequireQuestCompletion,
            setValue: value => config().RequireQuestCompletion = value,
            name: I18n.Gmcm_Gameplay_RequireQuestCompletion_Name,
            tooltip: I18n.Gmcm_Gameplay_RequireQuestCompletion_Tooltip
        );
        gmcm.AddEnum(
            mod,
            getValue: () => config().Scheduling,
            setValue: value => config().Scheduling = value,
            name: I18n.Gmcm_Gameplay_Scheduling_Name,
            tooltip: I18n.Gmcm_Gameplay_Scheduling_Tooltip
        );
        gmcm.AddNumberOption(
            mod,
            getValue: () => config().FriendshipMultiplier,
            setValue: value => config().FriendshipMultiplier = value,
            name: I18n.Gmcm_Gameplay_FriendshipMultiplier_Name,
            tooltip: I18n.Gmcm_Gameplay_FriendshipMultiplier_Tooltip,
            min: 0.05f,
            max: 1.0f,
            interval: 0.05f,
            formatValue: value => value.ToString("P0")
        );
        gmcm.AddBoolOption(
            mod,
            getValue: () => config().DetailedReturnReasons,
            setValue: value => config().DetailedReturnReasons = value,
            name: I18n.Gmcm_Gameplay_DetailedReturnReasons_Name,
            tooltip: I18n.Gmcm_Gameplay_DetailedReturnReasons_Tooltip
        );
    }
}
