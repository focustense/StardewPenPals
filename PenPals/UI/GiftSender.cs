using PenPals.Data;
using StardewValley.Menus;

namespace PenPals.UI;

/// <summary>
/// Initiates sending of gifts, i.e. when an NPC is selected from the gift mail menu.
/// </summary>
/// <param name="sender">The player sending the gift.</param>
/// <param name="item">The item to be gifted.</param>
/// <param name="config">Current mod configuration.</param>
/// <param name="data">Current mod data for the <paramref name="sender"/>.</param>
/// <param name="monitor">Game logger instance.</param>
public class GiftSender(
    Farmer sender,
    Item item,
    ModConfig config,
    GiftMailData data,
    IMonitor monitor
)
{
    /// <summary>
    /// Sends the current item to the specified <paramref name="npc"/>, subject to confirmation if configured.
    /// </summary>
    /// <param name="npc">The intended recipient of the gift.</param>
    public void Send(NPC npc)
    {
        Game1.playSound("smallSelect");
        if (config.RequireConfirmation)
        {
            Game1.playSound("breathin");
            var confirmationMessage = data.OutgoingGifts.TryGetValue(npc.Name, out var previousGift)
                ? I18n.GiftConfirmation_Replace(
                    previousGift.DisplayName,
                    item.DisplayName,
                    npc.displayName
                )
                : I18n.GiftConfirmation_New(item.DisplayName, npc.displayName);
            Game1.activeClickableMenu = new ConfirmationDialog(
                confirmationMessage,
                _ => ScheduleSend(npc)
            );
        }
        else
        {
            ScheduleSend(npc);
        }
    }

    private void ScheduleSend(NPC npc)
    {
        if (sender.ActiveItem != item)
        {
            monitor.Log(
                $"Couldn't schedule gift: player's {nameof(Farmer.ActiveItem)} no longer matches "
                    + "the gifted item.",
                LogLevel.Error
            );
            Game1.showRedMessage(I18n.Hud_Error_ScheduleGift());
            return;
        }
        if (sender.ActiveItem.getOne() is not SObject giftObject)
        {
            monitor.Log(
                $"Couldn't schedule gift: the active item {item.QualifiedItemId} is not an Object type.",
                LogLevel.Error
            );
            Game1.showRedMessage(I18n.Hud_Error_ScheduleGift());
            return;
        }
        sender.reduceActiveItemByOne();
        if (data.OutgoingGifts.TryGetValue(npc.Name, out var previousGiftObject))
        {
            sender.addItemByMenuIfNecessary(previousGiftObject);
        }
        data.OutgoingGifts[npc.Name] = giftObject;
        Game1.playSound("Ship");
        monitor.Log(
            $"Scheduled send of {item.Name} (quality {item.Quality}) to {npc.Name}.",
            LogLevel.Debug
        );
        Game1.exitActiveMenu();
        Game1.addHUDMessage(
            HUDMessage.ForCornerTextbox(
                I18n.Hud_Confirm_GiftSent(
                    item.DisplayName,
                    npc.displayName,
                    I18n.GetByKey($"Hud.Schedule.{config.Scheduling}")
                )
            )
        );
    }
}
