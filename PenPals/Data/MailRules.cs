﻿using StardewValley;
using StardewValley.Characters;
using StardewValley.Quests;

namespace PenPals.Data;

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
    /// <param name="questId">ID of the quest, if any, to be completed with the gift.</param>
    /// <returns>A <see cref="NonGiftableReasons"/> flag value including all reasons why the gift
    /// cannot be sent.</returns>
    public NonGiftableReasons CheckGiftability(
        Farmer from,
        NPC to,
        Item item,
        string? questId = null
    )
    {
        var reasons = NonGiftableReasons.None;
        if (!string.IsNullOrEmpty(questId))
        {
            var quest = GiftDistributor.GetDeliveryQuest(from, questId);
            if (quest is null)
            {
                reasons |= NonGiftableReasons.QuestMissing;
            }
            else if (quest.completed.Value)
            {
                reasons |= NonGiftableReasons.QuestCompleted;
            }
            // Don't process additional reasons for quests, since quests are not normal gifts.
            return reasons;
        }
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
        if (to.SpeaksDwarvish() && !from.canUnderstandDwarves)
        {
            reasons |= NonGiftableReasons.NoDwarvish;
        }
        if (
            to.TryGetDialogue("reject_" + item.ItemId) is not null
            || to.TryGetDialogue("RejectItem_" + item.ItemId) is not null
        )
        {
            reasons |= NonGiftableReasons.Rejection;
        }
        if (!from.friendshipData.TryGetValue(to.Name, out var friendship))
        {
            reasons |= NonGiftableReasons.Unmet;
            return reasons;
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
                && !WillReceiveOnBirthday(to)
            )
            {
                reasons |= NonGiftableReasons.WeeklyLimit;
            }
        }
        return reasons;
    }

    /// <summary>
    /// Estimates (with high accuracy) the amount of friendship gain expected from
    /// <see cref="NPC.receiveGift"/>.
    /// </summary>
    /// <param name="from">The player sending the gift.</param>
    /// <param name="to">The NPC who will receive the gift.</param>
    /// <param name="item">The item being given as gift.</param>
    /// <param name="quest">The active quest, if any, to be completed.</param>
    /// <returns></returns>
    public int EstimateFriendshipGain(Farmer from, NPC to, Item item, Quest? quest)
    {
        if (quest is not null)
        {
            return GetQuestPoints(quest);
        }
        float multiplier = GetQualityMultiplier(item.Quality);
        if (WillReceiveOnBirthday(to))
        {
            multiplier *= 8f;
        }
        if (to.getSpouse() == from)
        {
            multiplier /= 2f;
        }
        var taste = to.getGiftTasteForThisItem(item);
        var (_, basePoints) = GiftTasteBehavior.ForGiftTaste(taste);
        if (basePoints > 0)
        {
            multiplier *= config.FriendshipMultiplier;
        }
        var resultPoints = (int)(basePoints * multiplier);
        int currentPoints = 0;
        if (from.friendshipData.TryGetValue(to.Name, out var friendship))
        {
            currentPoints = friendship.Points;
            var maxPoints = GetMaxFriendship(friendship, to.datable.Value);
            var remainingPoints = Math.Max(maxPoints - currentPoints, 0);
            resultPoints = Math.Min(resultPoints, remainingPoints);
        }
        if (resultPoints < 0)
        {
            resultPoints = Math.Max(resultPoints, -currentPoints);
        }
        return resultPoints;
    }

    /// <summary>
    /// Computes the points to be awarded for completing a quest, after applying scaling.
    /// </summary>
    /// <param name="quest">The quest to complete.</param>
    /// <returns>Scaled points awarded for completing the <paramref name="quest"/>.</returns>
    public int GetQuestPoints(Quest quest)
    {
        int basePoints = quest.dailyQuest.Value ? 150 : 255;
        return (int)(basePoints * config.QuestFriendshipMultiplier);
    }

    /// <summary>
    /// Checks if the recipient is already at max friendship with the specified gift-giver.
    /// </summary>
    /// <param name="from">The player sending the gift.</param>
    /// <param name="to">The NPC who will receive the gift.</param>
    /// <returns><c>true</c> if the <paramref name="from"/> player has the maximum allowed
    /// friendship (hearts) with the <paramref name="to"/> NPC, otherwise <c>false</c>.</returns>
    public bool HasMaxFriendship(Farmer from, NPC to)
    {
        return from.friendshipData.TryGetValue(to.Name, out var friendship)
            && friendship.Points >= GetMaxFriendship(friendship, to.datable.Value);
    }

    /// <summary>
    /// Checks whether the NPC will receive the gift on his/her birthday, given the current world
    /// date and configuration settings (<see cref="ModConfig.Scheduling"/>).
    /// </summary>
    /// <param name="npc">The NPC who will receive the gift.</param>
    /// <returns><c>true</c> if the gift would be received on the NPC's birthday, otherwise
    /// <c>false</c>.</returns>
    public bool WillReceiveOnBirthday(NPC npc)
    {
        var comparisonDate =
            config.Scheduling == GiftShipmentScheduling.SameDay
                ? Game1.Date
                : WorldDate.ForDaysPlayed(Game1.Date.TotalDays + 1);
        return IsBirthday(npc, comparisonDate);
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

    private static float GetQualityMultiplier(int quality)
    {
        return quality switch
        {
            1 => 1.1f,
            2 => 1.25f,
            4 => 1.5f,
            _ => 1.0f,
        };
    }

    private static bool IsBirthday(NPC npc, WorldDate date)
    {
        return npc.Birthday_Season == date.SeasonKey && npc.Birthday_Day == date.DayOfMonth;
    }
}
