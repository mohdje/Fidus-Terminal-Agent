using System.Diagnostics;
using Spectre.Console;
using Fidus.Enums;
using Fidus.Models;

namespace Fidus.Utils
{
    public class AppStart(ConsoleHelper consoleHelper)
    {
        readonly ConsoleHelper consoleHelper = consoleHelper;

        public async Task<AgentSettings?> Initialize(string[] commandArgs)
        {

            if (commandArgs.HasArgument(CommandArgId.Help))
            {
                CommandArgsExtension.ShowHelp();
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.Version))
            {
                AnsiConsole.MarkupLine("[bold]FIDUS version: 2.0.1[/]");
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.Logs))
            {
                if (File.Exists(AppFiles.ErrorLogsFilePath))
                {
                    var logs = File.ReadAllText(AppFiles.ErrorLogsFilePath);
                    AnsiConsole.Write(new Text(logs));
                    AnsiConsole.WriteLine();
                }
                else
                {
                    AnsiConsole.MarkupLine("[italic]No logs found.[/]");
                }

                return null;
            }

            var agentName = commandArgs.GetArgumentValue(CommandArgId.AgentName) ?? "terminal";

            var agentsSettingsManager = new AgentsSettingsManager();
            var agentSettings = agentsSettingsManager.GetAgentSettings(agentName);

            if (commandArgs.HasArgument(CommandArgId.ListAgents))
            {
                var agentsNames = agentsSettingsManager.GetAllAgentNames();
                if (agentsNames.Length == 0)
                {
                    AnsiConsole.MarkupLine("[italic]No agents found.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold cyan]Existing agents:[/]");
                    foreach (var name in agentsNames)
                        AnsiConsole.MarkupLine(name);
                }

                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.RemoveAgent))
            {
                var agentNameToRemove = commandArgs.GetArgumentValue(CommandArgId.AgentName);
                if (string.IsNullOrEmpty(agentNameToRemove))
                {
                    AnsiConsole.MarkupLine("[red]Please specify an agent name to remove using -a or --agent-name.[/]");
                    return null;
                }
                else if (agentNameToRemove == "terminal")
                {
                    AnsiConsole.MarkupLine("[red]The default terminal agent cannot be removed.[/]");
                    return null;
                }

                var agentSettingsToRemove = agentsSettingsManager.GetAgentSettings(agentNameToRemove);
                if (agentSettingsToRemove is null)
                {
                    AnsiConsole.MarkupLine($"[red]Agent '{Markup.Escape(agentNameToRemove)}' not found.[/]");
                    return null;
                }

                agentsSettingsManager.RemoveAgentSettings(agentSettingsToRemove.Name);
                agentsSettingsManager.SaveSettings();
                AnsiConsole.MarkupLine($"[green]Agent '{Markup.Escape(agentNameToRemove)}' removed successfully.[/]");
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.AgentSettings))
            {
                if (agentSettings is null)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(agentName)} agent not found. Please check your settings.[/]");
                }
                else
                {
                    var table = new Table().Border(TableBorder.Rounded).AddColumn("Setting").AddColumn("Value");
                    table.AddRow("Name", agentSettings.Name);
                    table.AddRow("Inference Provider", agentSettings.InferenceProvider?.ToString() ?? "not set");
                    table.AddRow("Model Name", agentSettings.ModelName ?? "not set");
                    table.AddRow("API Token", agentSettings.MaskedApiToken ?? "not set");
                    table.AddRow("Temperature", agentSettings.Temperature?.ToString() ?? "not set");
                    table.AddRow("TopP", agentSettings.TopP?.ToString() ?? "not set");
                    AnsiConsole.Write(table);
                }

                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.Setup))
            {
                agentSettings ??= agentsSettingsManager.CreateAgentSettings(agentName);
                await SetupAgentSettingsAsync(agentSettings, consoleHelper);
                agentsSettingsManager.SaveSettings();
                AnsiConsole.MarkupLine($"[bold green]{Markup.Escape(agentSettings.Name)} agent saved successfully.[/]");
                AnsiConsole.MarkupLine($"You can read/edit the system prompt of [bold]{Markup.Escape(agentSettings.Name)}[/] agent with the command [bold cyan]fidus -sp -a {Markup.Escape(agentSettings.Name)}[/]");
                return null;
            }
            else if (commandArgs.HasArgument(CommandArgId.SystemPrompt))
            {
                if (agentName == "terminal")
                {
                    AnsiConsole.MarkupLine("[red]The default terminal agent's system prompt cannot be modified.[/]");
                }
                else if (agentSettings is null)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(agentName)} agent not found. Please check your settings.[/]");
                }
                else
                {
                    var systemPromptFilePath = AppFiles.GetSystemPromptFile(agentSettings.Id);
                    if (!File.Exists(systemPromptFilePath))
                    {
                        AnsiConsole.MarkupLine($"[red]System prompt file for agent '{Markup.Escape(agentSettings.Name)}' was not found.[/]");
                    }
                    else
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(systemPromptFilePath) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.WriteLine($"Could not open the system prompt file: {ex.Message}");
                        }
                    }
                }

                return null;
            }
            else if (agentSettings is null)
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(agentName)} agent not found. Please check your settings.[/]");
                return null;
            }

            return agentSettings;
        }

        static async Task SetupAgentSettingsAsync(AgentSettings agentSettings, ConsoleHelper consoleHelper)
        {
            AnsiConsole.MarkupLine($"[bold blue]Let's set up {Markup.Escape(agentSettings.Name)} agent[/]");
            AnsiConsole.WriteLine();

            var inferenceProviders = Enum.GetNames<InferenceProvider>();
            if (agentSettings.InferenceProvider.HasValue)
                inferenceProviders[(int)agentSettings.InferenceProvider.Value] += " (current)";

            var inferenceProviderIndex = consoleHelper.GetUserChoice(
                "What inference provider do you want to use ?",
                inferenceProviders);

            agentSettings.InferenceProvider = (InferenceProvider)inferenceProviderIndex;
            var selectedInferenceProviderName = agentSettings.InferenceProvider.ToString();

            AnsiConsole.WriteLine();

            var apiToken = consoleHelper.GetSecretInput(
                $"Enter your API key for {selectedInferenceProviderName} (press Enter to keep the existing value):",
                agentSettings.ApiToken ?? string.Empty);

            if (!string.IsNullOrEmpty(apiToken) && !string.Equals(apiToken, agentSettings.ApiToken, StringComparison.Ordinal))
                agentSettings.ApiToken = apiToken;

            AnsiConsole.WriteLine();

            agentSettings.ModelName = consoleHelper.GetUserInput(
                $"Enter the model name to use for {selectedInferenceProviderName}:",
                agentSettings.ModelName);

            AnsiConsole.WriteLine();

            agentSettings.Temperature = consoleHelper.GetUserDecimal(
                $"Enter the temperature to use for {agentSettings.ModelName} model (value between 0 and 2)",
                0, 2, agentSettings.Temperature);

            AnsiConsole.WriteLine();

            agentSettings.TopP = consoleHelper.GetUserDecimal(
                $"Enter the top-p value to use for {agentSettings.ModelName} model (value between 0 and 1)",
                0, 1, agentSettings.TopP);

            AnsiConsole.WriteLine();

            if (agentSettings.Id != 0)
            {
                var description = consoleHelper.GetUserInput(
                    $"Describe the task {agentSettings.Name} agent is designed to perform:",
                    agentSettings.Description);

                if (description != agentSettings.Description && !string.IsNullOrEmpty(description))
                {
                    AnsiConsole.WriteLine();
                    agentSettings.Description = description;

                    var aiClient = await Agent.Agent.CreateAsync(agentSettings);
                    var prompt = $@"Write a high-quality system prompt to guide an AI agent dedicated to achieve the following purpose: {agentSettings.Description}. 
            Agent should also be able to answer questions about its work and questions relative to its domain of expertise. The agent shall not answer questions that are not related to its purpose.
            No reasoning, no explanation, just the system prompt in a markdown format. Do not include any additional text. ";

                    try
                    {
                        var response = await consoleHelper.RunWithStatusAsync(
                            "Creating agent",
                            async () => await aiClient.Invoke(prompt));

                        var systemPromptFilePath = AppFiles.GetSystemPromptFile(agentSettings.Id);
                        await File.WriteAllTextAsync(systemPromptFilePath, response);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteLine($"Failed to generate the system prompt: {ex.Message}");
                    }
                }
            }
        }
    }
}
