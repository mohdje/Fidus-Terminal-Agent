using ConsoleInk;
using Fidus.Enums;
using Fidus.Models;

namespace Fidus.Utils
{
    public class AppStart(ConsoleHelper consoleHelper)
    {
        readonly ConsoleHelper consoleHelper = consoleHelper;

        public async Task<AgentSettings> Initialize(string[] commandArgs)
        {
            var agentName = commandArgs.GetArgumentValue(CommandArgId.AgentName) ?? "terminal";

            if (commandArgs.HasArgument(CommandArgId.Help))
            {
                CommandArgsExtension.ShowHelp();
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.Version))
            {
                Console.WriteLine($"FIDUS version: 2.0.0");
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.Logs))
            {
                if (File.Exists(AppFiles.ErrorLogsFilePath))
                {
                    var logs = File.ReadAllText(AppFiles.ErrorLogsFilePath);
                    Console.WriteLine(logs);
                }
                else
                    Console.WriteLine("No logs found.");

                return null;
            }

            var agentsSettingsManager = new AgentsSettingsManager();
            var agentSettings = agentsSettingsManager.GetAgentSettings(agentName);
            if (commandArgs.HasArgument(CommandArgId.ListAgents))
            {
                var agentsNames = agentsSettingsManager.GetAllAgentNames();
                if (agentsNames.Length == 0)
                    Console.WriteLine("No agents found.");
                else
                {
                    Console.WriteLine("Existing agents:");
                    foreach (var name in agentsNames)
                        Console.WriteLine($"- {name}");
                }

                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.RemoveAgent))
            {
                var agentNameToRemove = commandArgs.GetArgumentValue(CommandArgId.AgentName);
                if (string.IsNullOrEmpty(agentNameToRemove))
                {
                    Console.WriteLine("Please specify an agent name to remove using -a or --agent-name.");
                    return null;
                }
                else if (agentNameToRemove == "terminal")
                {
                    Console.WriteLine("The default terminal agent cannot be removed.");
                    return null;
                }

                var agentSettingsToRemove = agentsSettingsManager.GetAgentSettings(agentNameToRemove);
                if (agentSettingsToRemove is null)
                {
                    Console.WriteLine($"Agent '{agentNameToRemove}' not found.");
                    return null;
                }

                agentsSettingsManager.RemoveAgentSettings(agentSettingsToRemove.Name);
                agentsSettingsManager.SaveSettings();
                Console.WriteLine($"Agent '{agentNameToRemove}' removed successfully.");
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.AgentSettings))
            {
                if (agentSettings is null)
                    Console.WriteLine($"{agentName} agent not found. Please check your settings.");
                else
                {
                    Console.WriteLine($"Settings for agent '{agentSettings.Name}':");
                    Console.WriteLine($"- Inference Provider: {agentSettings.InferenceProvider}");
                    Console.WriteLine($"- Model Name: {agentSettings.ModelName}");
                    Console.WriteLine($"- API Token: {agentSettings.ApiToken[..4]}****{agentSettings.ApiToken[^4..]}");
                    Console.WriteLine($"- Temperature: {agentSettings.Temperature}");
                    Console.WriteLine($"- TopP: {agentSettings.TopP}");
                }

                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.Setup))
            {
                agentSettings ??= agentsSettingsManager.CreateAgentSettings(agentName);
                await SetupAgentSettingsAsync(agentSettings, consoleHelper);
                agentsSettingsManager.SaveSettings();
                Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightGreen}{agentSettings.Name}{Ansi.Reset} {Ansi.Bold}{Ansi.FgBlue}agent saved successfully{Ansi.Reset}");
                return null;
            }
            else if (agentSettings is null)
            {
                Console.WriteLine($"{agentName} agent not found. Please check your settings.");
                return null;
            }

            return agentSettings;
        }

        static async Task SetupAgentSettingsAsync(AgentSettings agentSettings, ConsoleHelper consoleHelper)
        {
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightBlue}Let's set up {Ansi.Reset}{Ansi.Bold}{Ansi.FgBrightGreen}{agentSettings.Name} {Ansi.Bold}{Ansi.FgBrightBlue}agent{Ansi.Reset}");
            Console.WriteLine();

            var inferenceProviders = Enum.GetNames<InferenceProvider>();
            var inferenceProviderIndex = consoleHelper.GetUserChoice($"What inference provider do you want to use ? (choose an index from the list below)", inferenceProviders, agentSettings.InferenceProvider.HasValue ? (int)agentSettings.InferenceProvider.Value : null);
            agentSettings.InferenceProvider = (InferenceProvider)inferenceProviderIndex;

            Console.WriteLine();

            agentSettings.ApiToken = consoleHelper.GetUserInput($"Enter your api key for {inferenceProviders[inferenceProviderIndex]}:", agentSettings.ApiToken);

            Console.WriteLine();

            agentSettings.ModelName = consoleHelper.GetUserInput($"Enter the model name to use for {inferenceProviders[inferenceProviderIndex]}:", agentSettings.ModelName);

            Console.WriteLine();

            agentSettings.Temperature = consoleHelper.GetUserInput($"Enter the temperature to use for {agentSettings.ModelName} model (value between 0 and 2)", 0, 2, agentSettings.Temperature);

            Console.WriteLine();

            agentSettings.TopP = consoleHelper.GetUserInput($"Enter the top-p value to use for {agentSettings.ModelName} model (value between 0 and 1)", 0, 1, agentSettings.TopP);

            Console.WriteLine();

            if (agentSettings.Id != 0)
            {
                var description = consoleHelper.GetUserInput($"Describe the task {agentSettings.Name} agent is designed to perform:", agentSettings.Description);

                if (description != agentSettings.Description && !string.IsNullOrEmpty(description))
                {
                    Console.WriteLine();
                    agentSettings.Description = description;

                    var aiClient = await Agent.Agent.CreateAsync(agentSettings);
                    var prompt = $@"Write a high-quality system prompt to guide an AI agent dedicated to achieve the following purpose: {agentSettings.Description}. 
            Agent should also be able to answer questions about its work and questions relative to its domain of expertise. The agent shall not answer questions that are not related to its purpose.
            No reasoning, no explanation, just the system prompt in a markdown format. Do not include any additional text. ";

                    consoleHelper.StartLoadingAnimationAsync("Creating agent");
                    var response = await aiClient.Invoke(prompt);
                    await consoleHelper.StopLoadingAnimationAsync();

                    var systemPromptFilePath = AppFiles.GetSystemPromptFile(agentSettings.Id);
                    await File.WriteAllTextAsync(systemPromptFilePath, response);
                }
            }
        }
    }
}