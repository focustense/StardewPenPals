namespace GiftMailer.Commands;

internal class CommandHandler
{
    private readonly HelpCommand helpCommand;
    private readonly IMonitor monitor;
    private readonly string root;
    private readonly Dictionary<string, Action<string[]>> runners = [];

    public CommandHandler(IMonitor monitor, string root)
    {
        this.monitor = monitor;
        this.root = root;
        helpCommand = new(monitor, root);
        AddCommand(helpCommand);
    }

    public void AddCommand<TArgs>(ICommand<TArgs> command)
    {
        runners.Add(command.Name, argsWithRoot => RunCommand(command, argsWithRoot));
        if (command != helpCommand)
        {
            helpCommand.AddCommand(command);
        }
        monitor.Log($"Registered command: {command.Name}", LogLevel.Debug);
    }

    public void RunCommand(string[] args)
    {
        if (args.Length == 0)
        {
            monitor.Log(
                $"No command specified. Type '{root} help' to see available commands.",
                LogLevel.Error
            );
            return;
        }
        var commandName = args[0];
        if (runners.TryGetValue(commandName, out var runner))
        {
            runner(args);
        }
        else
        {
            monitor.Log(
                $"Invalid command '{commandName}'. Type '{root} help' to see available commands.",
                LogLevel.Error
            );
        }
    }

    private void RunCommand<TArgs>(ICommand<TArgs> command, string[] argsWithRoot)
    {
        var argsWithoutRoot = argsWithRoot[1..];
        if (command.TryParseArgs(argsWithoutRoot, out var parsedArgs, out var error))
        {
            command.Execute(parsedArgs);
        }
        else
        {
            monitor.Log($"Error running command '{command.Name}': {error}", LogLevel.Error);
        }
    }
}
