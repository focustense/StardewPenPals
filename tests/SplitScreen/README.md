# Split-screen regression checks

Run with a .NET 8 SDK (no game or external test packages required):

```sh
dotnet run --project tests/SplitScreen -c Release
```

This links the production `GiftDistributor.cs` against small game/SMAPI boundary
doubles. It checks ownership of queued mail, local quest completion instead of
global farmer replicas, friendship attribution, actual quest point logging,
idempotence, returns, offline mail preservation, and reversed screen event order.
It does not emulate the SMAPI event loop, save serialization, or the complete game.

An original distributor can be supplied with `-p:DistributorSource=/absolute/path/to/original.cs`
to confirm the regression fails before the fix.

In-game verification still required:

1. Test both SameDay and NextDay schedules with two local players sending to the
   same NPC. Only each sender's gift counters and friendship should change.
2. Have player two mail an item for an accepted ItemDeliveryQuest. Verify the
   quest completes and rewards, follow-up quests, and statistics belong to player two.
   Also test a FishingQuest after catching the required fish.
3. Queue mail on player one before player two joins. Joining must not replace the
   shared queue. Test both players' mail across saving and reloading.
4. Make a quest unavailable before delivery and verify only its sender gets the
   return item. Repeat a delivery command to verify no duplicate award occurs.
5. With player two connected, `gm receiveall screen=1` processes only that screen's
   queue; the `Pts` column reports the actual friendship delta for gifts and quests.
   This command immediately delivers parcels and changes game state.
