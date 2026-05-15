using Fidus;
using ConsoleInk;
using Fidus.Utils;
using Fidus.Agent;

var commandArgs = Environment.GetCommandLineArgs();
if (commandArgs.Any(arg => arg == "-h" || arg == "--help"))
{
    CommandArgsExtension.ShowHelp();
    return;
}
if (commandArgs.Any(arg => arg == "-v" || arg == "--version"))
{
    Console.WriteLine("Fidus CLI version 1.0.0");
    return;
}

var settings = SettingsManager.Init(commandArgs);

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

        Console.WriteLine();

        var animationCancellationTokenSource = new CancellationTokenSource();

        try
        {
            consoleDrawer.StartLoadingAnimationAsync("Thinking...");

            await Task.Delay(2000);

            var response = await aiAgent.Invoke(userInput);

            await consoleDrawer.StopLoadingAnimationAsync();

            Console.WriteLine(MarkdownFormatter.FormatDocument(response));
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            await consoleDrawer.StopLoadingAnimationAsync();
            Console.WriteLine();
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightRed}Something went wrong, please try again.{Ansi.Reset}");
        }
    }
}

