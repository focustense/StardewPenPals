using PenPals.Data;
using StardewValley.Quests;

// Exercise the production distributor with a small game boundary double. In particular,
// questComplete uses the current screen, as the real game's implementation does.
var first = new Farmer(1, "Host");
var second = new Farmer(2, "Farmhand");
Game1.Farmers.Add(1, first);
Game1.Farmers.Add(2, second);
var npc = new NPC("Robin");
Game1.Npcs.Add(npc.Name, npc);
var data = new ModData();
data.FarmerGiftMail.Add(1, new());
data.FarmerGiftMail.Add(2, new());
data.FarmerGiftMail.Add(3, new()); // offline farmer
var content = new ContentHelper();
var distributor = new GiftDistributor(new(new(), data, new(), new()), content);
var quest = new Quest("quest-2");
second.questLog.Add(quest);
data.FarmerGiftMail[1].OutgoingGifts.Add(npc.Name, new(new(), null));
data.FarmerGiftMail[2].OutgoingGifts.Add(npc.Name, new(new(), quest.Id));
data.FarmerGiftMail[3].OutgoingGifts.Add(npc.Name, new(new(), null));

Game1.player = first;
var hostResults = distributor.ReceiveAll();
Check(hostResults.Count == 1 && hostResults[0].From == first, "host processes only its mail");
Check(!quest.completed.Value && data.FarmerGiftMail[2].OutgoingGifts.Count == 1,
    "host leaves farmhand's quest and parcel untouched");
Check(first.Points == 80 && second.Points == 0, "gift friendship belongs to sender");
Check(data.FarmerGiftMail[3].OutgoingGifts.Count == 1, "offline player's parcel remains queued");

Game1.player = second;
// Global farmer lookup may expose a network replica with the same ID. Its quest log
// is not the local player's authoritative quest log.
Game1.Farmers[2] = new Farmer(2, "Farmhand replica");
var questResults = distributor.ReceiveAll();
Check(questResults.Count == 1 && quest.completed.Value, "farmhand completes its quest");
Check(second.QuestsCompleted == 1 && first.QuestsCompleted == 0,
    "quest statistics belong to farmhand's screen");
Check(second.Points == 150 && first.Points == 80, "quest friendship belongs to farmhand");
Check(data.FarmerGiftMail[2].OutgoingGifts.Count == 0, "completed parcel removed");
Check(distributor.ReceiveAll().Count == 0 && second.Points == 150,
    "repeated delivery cannot award twice");

// Logging must report actual gains, including modifiers/caps applied by the game.
second.Points = 490;
second.PointLimit = 500;
var cappedQuest = new Quest("capped");
second.questLog.Add(cappedQuest);
data.FarmerGiftMail[2].OutgoingGifts.Add(npc.Name, new(new(), cappedQuest.Id));
var cappedResults = distributor.ReceiveAll();
Check(cappedResults.Single().Points == 10, "quest log reports actual friendship delta");

data.FarmerGiftMail[2].OutgoingGifts.Add(npc.Name, new(new(), "missing"));
var returns = distributor.ReceiveAll();
Check(returns.Single().Points == 0 && second.mailbox.Count == 1 && first.mailbox.Count == 0,
    "missing quest returns parcel only to its sender");
Check(content.Invalidations == 1 && data.FarmerGiftMail[2].ReturnedGifts.Count == 1,
    "return is available in mail content");

// Reverse screen-event ordering, with both players gifting to the same NPC.
second.PointLimit = int.MaxValue;
data.FarmerGiftMail[1].OutgoingGifts.Add(npc.Name, new(new(), null));
data.FarmerGiftMail[2].OutgoingGifts.Add(npc.Name, new(new(), null));
var secondFirst = distributor.ReceiveAll();
Check(secondFirst.Count == 1 && data.FarmerGiftMail[1].OutgoingGifts.Count == 1,
    "farmhand-first event order preserves host mail");
Game1.player = first;
Check(distributor.ReceiveAll().Count == 1 && first.Points == 160 && second.Points == 580,
    "both screens receive their own gift points exactly once");
Console.WriteLine("PASS: all split-screen distributor checks.");

static void Check(bool condition, string name)
{
    if (!condition)
        throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
}
