using ConsoleInk;
using Fidus.Utils;
using Fidus.Agent;

var commandArgs = Environment.GetCommandLineArgs();
var hasReadOnlyArgs = CommandArgsExtension.HasReadOnlyArgs(commandArgs);
if (hasReadOnlyArgs)
    return;
var settings = SettingsManager.Init(commandArgs);
if (commandArgs.Any(arg => arg == "-s" || arg == "--settings"))
{
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightYellow}Current settings:{Ansi.Reset}");
    Console.WriteLine($"Inference Provider: {settings.InferenceProvider}");
    Console.WriteLine($"Model Name: {settings.ModelName}");
    Console.WriteLine($"API Token: {settings.ApiToken[..4]}****{settings.ApiToken[^4..]}");
    Console.WriteLine($"Temperature: {settings.Temperature}");
    Console.WriteLine($"TopP: {settings.TopP}");
    return;
}

Agent agent;
var consoleDrawer = new ConsoleHelper();
try
{
    agent = new Agent(settings, consoleDrawer);
}
catch (Exception ex)
{
    Console.WriteLine("An error occured during AI Agent initialization: " + ex.Message);
    return;
}

if (agent is not null)
    await Start(agent, consoleDrawer);


static async Task Start(Agent aiAgent, ConsoleHelper consoleHelper)
{
    Console.WriteLine();

    consoleHelper.DrawLogo();

    Console.WriteLine();
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgMagenta}          FIDUS{Ansi.Reset}");
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}     Your AI assistant{Ansi.Reset}");

    Console.WriteLine();
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgWhite} Hello {Ansi.Bold}{Ansi.FgCyan}{Environment.UserName}{Ansi.Reset}{Ansi.Bold}{Ansi.FgWhite}, what can I do for you ? {Ansi.Reset}");

    while (true)
    {
        var userInput = consoleHelper.GetUserInput();
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
            File.AppendAllText("error.log", $"[{DateTime.Now}] Error: {ex.Message}{Environment.NewLine}");
            Console.WriteLine();
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightRed}Something went wrong, please try again. Read logs with -l or --logs for details.{Ansi.Reset}");

            Console.WriteLine($"Error details: {ex.Message}");
        }
    }
}



