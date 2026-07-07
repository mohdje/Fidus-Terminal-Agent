using ConsoleInk;
using Fidus.Utils;
using Fidus.Agent;
using Fidus.Enums;
using PromptVit;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var commandArgs = Environment.GetCommandLineArgs();

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
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgMagenta}          FIDUS{Ansi.Reset}");
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}     Your {aiAgent.Name} assistant{Ansi.Reset}");

    Console.WriteLine();
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgWhite} Hello {Ansi.Bold}{Ansi.FgCyan}{Environment.UserName}{Ansi.Reset}{Ansi.Bold}{Ansi.FgWhite}, what can I do for you ? {Ansi.Reset}");

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

            var r = MarkdownConsole.Render(response);
            Console.WriteLine(r);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            await consoleHelper.StopLoadingAnimationAsync();
            File.AppendAllText(AppFiles.ErrorLogsFilePath, $"[{DateTime.Now}] Error: {ex.Message}{Environment.NewLine}");
            Console.WriteLine();
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightRed}Something went wrong, please try again. Read logs with -l or --logs for details.{Ansi.Reset}");

            Console.WriteLine($"Error details: {ex.Message}");
        }
    }
}

