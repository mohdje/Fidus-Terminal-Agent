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
{
    var updateAvailable = await new GitHubReleaseUpdateChecker().CheckForUpdateAsync();

    await Start(agent, consoleHelper, loadHistory, updateAvailable);
}

static async Task Start(Agent aiAgent, ConsoleHelper consoleHelper, bool loadHistory, bool updateAvailable)
{
    Console.WriteLine();

    consoleHelper.RenderWelcomeScreen(aiAgent.Name);

    if (loadHistory)
        await consoleHelper.LoadChatHistoryAsync(aiAgent);
    else
    {
        consoleHelper.RenderNewSessionMessage(aiAgent.Name);
    }

    if (updateAvailable)
        consoleHelper.RenderUpdateAvailable();

    while (true)
    {
        var userInput = consoleHelper.GetUserPrompt();
        if (string.IsNullOrEmpty(userInput))
            break;

        Console.WriteLine();

        int? id = null;
        try
        {
            id = await consoleHelper.RenderTaskProcessingAsync("Thinking");

            var response = await aiAgent.Invoke(userInput);

            await consoleHelper.StopRenderTaskProcessingAsync(id.GetValueOrDefault(-1));

            Console.WriteLine();

            consoleHelper.RenderMarkdown(response);

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            await consoleHelper.StopRenderTaskProcessingAsync(id.GetValueOrDefault(-1));
            consoleHelper.RenderError(ex.Message);
            File.AppendAllText(AppFiles.ErrorLogsFilePath, $"[{DateTime.Now}] Error: {ex.Message}{Environment.NewLine}");
        }
    }
}