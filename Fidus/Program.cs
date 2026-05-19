using Fidus;
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
    Console.WriteLine($"API Token: {settings.ApiToken}");
    Console.WriteLine($"Temperature: {settings.Temperature}");
    Console.WriteLine($"TopP: {settings.TopP}");
    return;
}

Agent agent;
var consoleDrawer = new ConsoleDrawer();
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


static async Task Start(Agent aiAgent, ConsoleDrawer consoleDrawer)
{
    Console.WriteLine();

    consoleDrawer.DrawLogo();

    Console.WriteLine();
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}   Fidus, your AI assistant");
    Console.WriteLine();
    Console.WriteLine($"{Ansi.Bold}{Ansi.FgWhite} Hello {Environment.UserName}, what can I do for you ? {Ansi.Reset}");

    while (true)
    {
        Console.Write("> ");

        var userInput = Console.ReadLine();

        if (string.IsNullOrEmpty(userInput))
            break;

        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.WriteLine($"> {Ansi.FgBrightMagenta}{userInput}{Ansi.Reset}");
        Console.WriteLine();

        try
        {
            consoleDrawer.StartLoadingAnimationAsync("Thinking");

            var response = await aiAgent.Invoke(userInput);

            await consoleDrawer.StopLoadingAnimationAsync();

            Console.WriteLine(MarkdownFormatter.FormatDocument(response));
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            await consoleDrawer.StopLoadingAnimationAsync();
            File.AppendAllText("error.log", $"[{DateTime.Now}] Error: {ex.Message}{Environment.NewLine}");
            Console.WriteLine();
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightRed}Something went wrong, please try again. Read logs with -l or --logs for details.{Ansi.Reset}");

            Console.WriteLine($"Error details: {ex.Message}");
        }
    }
}

