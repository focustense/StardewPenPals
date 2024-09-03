using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace GiftMailer.Commands;

internal record HelpArgs(
    [property: Optional]
    [property: Description("Command to get help for")]
        string? CommandName = null
);

internal class HelpCommand : ICommand<HelpArgs>
{
    public string Name => "help";

    public string Description => "Print help about a command.";

    private readonly Dictionary<string, CommandInfo> commands = [];
    private readonly IMonitor monitor;
    private readonly string root;

    public HelpCommand(IMonitor monitor, string root)
    {
        this.monitor = monitor;
        this.root = root;
        AddCommand(this);
    }

    public void AddCommand<TArgs>(ICommand<TArgs> command)
    {
        var arguments = typeof(TArgs).GetProperties().Select(GetArgumentInfo).ToList();
        var commandInfo = new CommandInfo(command, arguments);
        commands.Add(command.Name, commandInfo);
    }

    public void Execute(HelpArgs args)
    {
        if (!string.IsNullOrEmpty(args.CommandName))
        {
            DisplayCommandHelp(args.CommandName);
        }
        else
        {
            DisplayAllCommands();
        }
    }

    public bool TryParseArgs(
        string[] args,
        [MaybeNullWhen(false)] out HelpArgs parsedArgs,
        [MaybeNullWhen(true)] out string error
    )
    {
        if (args.Length > 1)
        {
            error = "Too many arguments.";
            parsedArgs = null;
            return false;
        }
        parsedArgs = new(args.Length > 0 ? args[0] : null);
        error = null;
        return true;
    }

    private void DisplayAllCommands()
    {
        var helpText = new StringBuilder();
        helpText.AppendLine("Available commands:");
        helpText.AppendLine();
        var commandWidth = commands.Keys.Max(name => name.Length);
        foreach (var commandInfo in commands.Values)
        {
            var command = commandInfo.Command;
            helpText.AppendLine(
                $"  {command.Name.PadRight(commandWidth)}    {command.Description}"
            );
        }
        monitor.Log(helpText.ToString(), LogLevel.Info);
    }

    private void DisplayCommandHelp(string commandName)
    {
        if (!commands.TryGetValue(commandName, out var commandInfo))
        {
            monitor.Log($"Unknown command '{commandName}'.", LogLevel.Error);
            return;
        }
        var helpText = new StringBuilder();
        helpText.AppendLine(commandInfo.Command.Description);
        helpText.AppendLine();
        helpText.Append($"Usage: {root} {commandName}");
        foreach (var arg in commandInfo.Arguments)
        {
            helpText.Append(' ');
            helpText.Append(arg.IsOptional ? "[" : "<");
            helpText.Append(arg.Name);
            helpText.Append(arg.IsOptional ? "]" : ">");
        }
        helpText.AppendLine();
        if (commandInfo.Arguments.Count > 0)
        {
            helpText.AppendLine();
            helpText.AppendLine("Options:");
            var argWidth = commandInfo.Arguments.Max(arg => arg.Name.Length);
            foreach (var arg in commandInfo.Arguments)
            {
                helpText.AppendLine($"  {arg.Name.PadRight(argWidth)}    {arg.Description}");
            }
        }
        monitor.Log(helpText.ToString(), LogLevel.Info);
    }

    private static ArgumentInfo GetArgumentInfo(PropertyInfo property)
    {
        var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
        var isOptional = property.GetCustomAttribute<OptionalAttribute>() is not null;
        return new(property.Name, description, isOptional);
    }
}

internal record CommandInfo(ICommand Command, IList<ArgumentInfo> Arguments);

internal record ArgumentInfo(string Name, string Description, bool IsOptional);
