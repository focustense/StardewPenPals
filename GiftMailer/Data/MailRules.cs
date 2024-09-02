using StardewValley.Characters;

namespace GiftMailer.Data;

/// <summary>
/// Rules for mailing gifts.
/// </summary>
/// <param name="customRules">The custom rule data loaded for the mod.</param>
public class MailRules(ModConfig config, CustomRules customRules)
{
    /// <summary>
    /// Checks whether a given item can be gifted to a given NPC.
    /// </summary>
    /// <param name="from">The player sending the gift.</param>
    /// <param name="to">The NPC who will receive the gift.</param>
    /// <param name="item">The item being given as gift.</param>
    /// <returns>A <see cref="NonGiftableReasons"/> flag value including all reasons why the gift
    /// cannot be sent.</returns>
    public NonGiftableReasons CheckGiftability(Farmer from, NPC to, Item item)
    {
        var reasons = NonGiftableReasons.None;
        if (!to.CanReceiveGifts())
        {
            reasons |= NonGiftableReasons.CannotReceiveGifts;
        }
        if (to is Child)
        {
            reasons |= NonGiftableReasons.Child;
        }
        if (to.getSpouse() == from)
        {
            reasons |= NonGiftableReasons.Spouse;
        }
        if (to.isDivorcedFrom(from))
        {
            reasons |= NonGiftableReasons.Divorced;
        }
        if (to.TryGetDialogue("reject_" + item.ItemId) is not null
            || to.TryGetDialogue("RejectItem_" + item.ItemId) is not null)
        {
            reasons |= NonGiftableReasons.Rejection;
        }
        if (!from.friendshipData.TryGetValue(to.Name, out var friendship))
        {
            reasons |= NonGiftableReasons.Unmet;
            return reasons;
        }
        if (friendship.Points >= GetMaxFriendship(friendship, to.datable.Value))
        {
            reasons |= NonGiftableReasons.MaxFriendship;
        }
        if (!customRules.IgnoreLimits.Contains(item.QualifiedItemId))
        {
            var sameDayShipping = config.Scheduling == GiftShipmentScheduling.SameDay;
            if (sameDayShipping && friendship.GiftsToday >= 1)
            {
                reasons |= NonGiftableReasons.DailyLimit;
            }
            if (
                friendship.GiftsThisWeek >= 2
                && (sameDayShipping || Game1.Date.DayOfWeek != DayOfWeek.Saturday)
                && !IsBirthday(
                    to,
                    sameDayShipping ? Game1.Date : WorldDate.ForDaysPlayed(Game1.Date.TotalDays + 1)
                )
            )
            {
                reasons |= NonGiftableReasons.WeeklyLimit;
            }
        }
        return reasons;
    }

    // Does the same thing as Utility.GetMaximumHeartsForCharacter but doesn't assume Game1.player.
    private static int GetMaxFriendship(Friendship friendship, bool isDatable)
    {
        int maxHearts = isDatable ? 8 : 10;
        if (friendship.IsMarried())
        {
            maxHearts = 14;
        }
        else if (friendship.IsDating())
        {
            maxHearts = 10;
        }
        return maxHearts * 250;
    }

    private static bool IsBirthday(NPC npc, WorldDate date)
    {
        return npc.Birthday_Season == date.SeasonKey && npc.Birthday_Day == date.DayOfMonth;
    }
}
