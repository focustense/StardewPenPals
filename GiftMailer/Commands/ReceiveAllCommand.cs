using System.Diagnostics.CodeAnalysis;
using PenPals.Data;
using PenPals.Logging;

namespace PenPals.Commands;

internal record ReceiveAllArgs();

internal class ReceiveAllCommand(Func<GiftDistributor> distributorFactory)
    : ICommand<ReceiveAllArgs>
{
    public string Name => "receiveall";

    public string Description =>
        "Make all NPCs receive their gifts immediately, ignoring mail schedules.";

    public void Execute(ReceiveAllArgs args)
    {
        var distributor = distributorFactory();
        var results = distributor.ReceiveAll();
        GiftLogger.LogResults(results, "Gift shipment results:", distributor.Context.Monitor);
    }

    public bool TryParseArgs(
        string[] args,
        [MaybeNullWhen(false)] out ReceiveAllArgs parsedArgs,
        [MaybeNullWhen(true)] out string error
    )
    {
        parsedArgs = new();
        error = null;
        return true;
    }
}
