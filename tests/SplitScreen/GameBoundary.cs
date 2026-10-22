// Test doubles for the game/SMAPI boundary; not a substitute for an in-game co-op test.
public class Farmer(long id, string name)
{
    public long UniqueMultiplayerID = id;
    public string Name = name;
    public List<StardewValley.Quests.Quest> questLog = [];
    public List<string> mailbox = [];
    public int Points;
    public int PointLimit = int.MaxValue;
    public int QuestsCompleted;
    public int? tryGetFriendshipLevelForNPC(string name) => Points;
    public void changeFriendship(int points, NPC npc) => Points = Math.Min(Points + points, PointLimit);
}

public class NPC(string name)
{
    public string Name = name;
    public int getGiftTasteForThisItem(Gift gift) => 0;
    public void receiveGift(Gift gift, Farmer farmer, float friendshipChangeMultiplier, bool showResponse)
        => farmer.changeFriendship((int)(80 * friendshipChangeMultiplier), this);
}

public class Gift;
public enum LogLevel { Error }
public interface IMonitor { void Log(string text, LogLevel level); }
public class Monitor : IMonitor { public void Log(string text, LogLevel level) { } }
public interface IGameContentHelper { void InvalidateCache(string key); }
public class ContentHelper : IGameContentHelper
{
    public int Invalidations;
    public void InvalidateCache(string key) => Invalidations++;
}
public static class Game1
{
    public static Farmer player = null!;
    public static Dictionary<long, Farmer> Farmers = [];
    public static Dictionary<string, NPC> Npcs = [];
    public static int Date = 1;
    public static Farmer? GetPlayer(long id) => Farmers.GetValueOrDefault(id);
    public static NPC? getCharacterFromName(string name) => Npcs.GetValueOrDefault(name);
    public static void playSound(string name) { }
}
public static class GamePatches { public static bool SuppressQuestSounds; }
public static class LocationPatches { public static bool SuppressGiftSounds; }
public class ModConfig { public float FriendshipMultiplier = 1; }

namespace StardewValley.Quests
{
    public class Flag { public bool Value; }
    public class Quest(string id)
    {
        public string Id = id;
        public Flag completed = new();
        public string SafeId() => Id;
        public void questComplete()
        {
            completed.Value = true;
            Game1.player.QuestsCompleted++;
        }
    }
}

namespace PenPals.Data
{
    public class ModData { public Dictionary<long, GiftMailData> FarmerGiftMail = []; }
    public class GiftMailData
    {
        public Dictionary<string, Parcel> OutgoingGifts = [];
        public Dictionary<string, ReturnedGift> ReturnedGifts = [];
        public static string GetReturnMailKey(string id) => id;
    }
    public record Parcel(Gift Gift, string? QuestId);
    public record ReturnedGift(string Npc, Gift Gift, int Date, int Reasons);
    public record GiftResult(Farmer From, NPC To, Gift Gift,
        StardewValley.Quests.Quest? Quest, string Outcome, int Points);
    public class RulesContext(ModConfig config, ModData data, MailRules rules, Monitor monitor)
    {
        public ModConfig Config = config;
        public ModData Data = data;
        public MailRules Rules = rules;
        public Monitor Monitor = monitor;
    }
    public class MailRules
    {
        public int GetQuestPoints(StardewValley.Quests.Quest quest) => 150;
        public int CheckGiftability(Farmer farmer, NPC npc, Gift gift, string? questId)
            => questId is not null && GiftDistributor.GetDeliveryQuest(farmer, questId) is null ? 1 : 0;
    }
    public static class GiftTasteBehavior
    {
        public static (string, int) ForGiftTaste(int taste) => ("Loved", 80);
    }
}
