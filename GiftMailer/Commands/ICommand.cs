using System.Diagnostics.CodeAnalysis;

namespace PenPals.Commands;

internal interface ICommand
{
    string Name { get; }
    string Description { get; }
}

internal interface ICommand<TArgs> : ICommand
{
    void Execute(TArgs args);

    bool TryParseArgs(
        string[] args,
        [MaybeNullWhen(false)] out TArgs parsedArgs,
        [MaybeNullWhen(true)] out string error
    );
}
