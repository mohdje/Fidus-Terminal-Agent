using Fidus.Utils;
using Fidus.Agent;
using Fidus.Enums;
using PromptVit;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string[] commandArgs = args;
var consoleHelper = new ConsoleHelper();

var appStart = new AppStart(consoleHelper);
var agentSettings = await appStart.Initialize(commandArgs);

if (agentSettings is null)
    return;

Agent agent;
var loadHistory = commandArgs.HasArgument(CommandArgId.Resume);
var tools = new List<IAITool>
{
    new BashCommandTool(consoleHelper),
    new EditFileTool(consoleHelper),
    new ReadFileTool(consoleHelper),
    new InternetSearchTool(consoleHelper)
};

try
{
    agent = await Agent.CreateAsync(agentSettings, loadHistory, tools);
}
catch (Exception ex)
{
    consoleHelper.RenderError($"An error occurred during AI agent initialization - {ex.Message}");
    return;
}

if (agent is not null)
    await Start(agent, consoleHelper, loadHistory);

static async Task Start(Agent aiAgent, ConsoleHelper consoleHelper, bool loadHistory)
{
    Console.WriteLine();

    consoleHelper.RenderWelcomeScreen(aiAgent.Name, loadHistory ? await aiAgent.GetChatHistoryAsync() : null);

    while (true)
    {
        var userInput = consoleHelper.GetUserPrompt();
        if (string.IsNullOrEmpty(userInput))
            break;

        Console.WriteLine();

        try
        {
            var response = await consoleHelper.RunWithStatusAsync("Thinking", async () => await aiAgent.Invoke(userInput));

            consoleHelper.RenderMarkdown(response);

            Console.WriteLine();
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            consoleHelper.RenderError(ex.Message);
            File.AppendAllText(AppFiles.ErrorLogsFilePath, $"[{DateTime.Now}] Error: {ex.Message}{Environment.NewLine}");
        }
    }
}
