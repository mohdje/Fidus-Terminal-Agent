using Spectre.Console;
using Fidus.Utils;
using Fidus.Agent;
using Fidus.Enums;
using PromptVit;

Console.OutputEncoding = System.Text.Encoding.UTF8;
string[] commandArgs = Environment.GetCommandLineArgs();

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
    Console.WriteLine("An error occured during AI Agent initialization: " + ex.Message);
    return;
}

if (agent is not null)
    await Start(agent, consoleHelper);

static async Task Start(Agent aiAgent, ConsoleHelper consoleHelper)
{
    Console.WriteLine();

    consoleHelper.DrawLogo();

    Console.WriteLine();
    AnsiConsole.MarkupLine($"[bold magenta]          FIDUS[/]");
    AnsiConsole.MarkupLine($"[bold magenta]     Your {aiAgent.Name} assistant[/]");

    Console.WriteLine();
    AnsiConsole.MarkupLine($"[bold white] Hello [bold cyan]{Environment.UserName}[/], what can I do for you ? [/]");

    while (true)
    {
        var userInput = consoleHelper.GetUserPrompt();
        if (string.IsNullOrEmpty(userInput))
            break;

        Console.WriteLine();

        try
        {
            consoleHelper.StartLoadingAnimationAsync("Thinking");
            var response = await aiAgent.Invoke(userInput);
            await consoleHelper.StopLoadingAnimationAsync();

            var r = ConsoleInk.MarkdownConsole.Render(response);
            Console.WriteLine(r);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            await consoleHelper.StopLoadingAnimationAsync();
            File.AppendAllText(AppFiles.ErrorLogsFilePath, $"[{DateTime.Now}] Error: {ex.Message}{Environment.NewLine}");
            AnsiConsole.MarkupLine($"[bold brightred]Something went wrong, please try again. Read logs with -l or --logs for details.[/]");

            Console.WriteLine($"Error details: {ex.Message}");
        }
    }
}
