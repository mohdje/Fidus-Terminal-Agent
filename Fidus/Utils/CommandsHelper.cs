using Fidus.Enums;

namespace Fidus.Utils
{
    public static class CommandArgsExtension
    {
        public static CommandArg[] ValidCommandArgs =>
        [
            new CommandArg(["-a", "--agent-name"], CommandArgId.AgentName, "Specify an agent name to launch. If not specified, the default terminal agent will be used.", "Agent name cannot be an empty string"),
            new CommandArg(["-as", "--agent-settings"], CommandArgId.AgentSettings, "Show settings for a specific agent. Specify an agent name with -a. If not specified, the default terminal agent will be used.", string.Empty),
            new CommandArg(["-r", "--resume"], CommandArgId.Resume, "Resume previous agent session. Specify an agent name with -a. If not specified, the default terminal agent will be used.", string.Empty),
            new CommandArg(["-s", "--setup"], CommandArgId.Setup, "Setup agent. Specify an agent name with -a. If not specified, the default terminal agent will be used.", string.Empty),
            new CommandArg(["-rm", "--remove-agent"], CommandArgId.RemoveAgent, "Remove an existing agent. Specify an agent name with -a.", string.Empty),
            new CommandArg(["-la", "--list-agents"], CommandArgId.ListAgents, "List all existing agents.", string.Empty),
            new CommandArg(["-l", "--logs"], CommandArgId.Logs, "Show logs", string.Empty),
            new CommandArg(["-h", "--help"], CommandArgId.Help, "Show help message", string.Empty),
            new CommandArg(["-v", "--version"], CommandArgId.Version, "Show version information", string.Empty),
        ];
        public static string GetArgumentValue(this string[]? commandArgs, CommandArgId argumentId)
        {
            if (commandArgs == null)
                return null;

            var arg = ValidCommandArgs.FirstOrDefault(a => a.Id == argumentId);
            if (arg == null)
                return null;

            var index = GetArgumentIndex(commandArgs, argumentId);
            if (index >= 0 && index < commandArgs.Length - 1)
                return commandArgs[index + 1];

            return null;
        }

        public static bool HasArgument(this string[]? commandArgs, CommandArgId argumentId)
        {
            if (commandArgs == null)
                return false;

            var arg = ValidCommandArgs.FirstOrDefault(a => a.Id == argumentId);
            if (arg == null)
                return false;

            return GetArgumentIndex(commandArgs, argumentId) >= 0;
        }

        private static int GetArgumentIndex(this string[]? commandArgs, CommandArgId argumentId)
        {
            if (commandArgs == null)
                return -1;

            var arg = ValidCommandArgs.FirstOrDefault(a => a.Id == argumentId);
            if (arg == null)
                return -1;

            var index = commandArgs.Select((arg, i) => new { arg, i })
                                .FirstOrDefault(x => arg.Names.Contains(x.arg))?.i ?? -1;

            return index;
        }
        public static void ShowHelp()
        {
            var argumentList = ValidCommandArgs.Select(arg => $"{string.Join(", ", arg.Names)}: {arg.Description}").ToList();

            Console.WriteLine(string.Join(Environment.NewLine, argumentList));
        }
    }
}
