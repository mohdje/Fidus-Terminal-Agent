using PromptVit;
using PromptVit.AIClients;
using Fidus;

var commandArgs = Environment.GetCommandLineArgs();

var settings = SettingsManager.ReadSettings();
settings ??= new Settings();

if (commandArgs.Any(arg => arg == "-h" || arg == "--help"))
{
    ShowHelp();
    return;
}
else if (commandArgs.HasValidCommandArgs())
{
    var values = commandArgs.ReadArgs();
    settings.InferenceProvider ??= values.InferenceProvider;
    settings.ModelName ??= values.ModelName;
    settings.ApiToken ??= values.ApiToken;
    settings.Temperature = settings.Temperature == 0 ? values.Temperature : settings.Temperature;
    settings.TopP = settings.TopP == 0 ? values.TopP : settings.TopP;
    SettingsManager.SaveSettings(settings);
}
else
{
    var aiAgent = BuildAIAgent(settings);

    if (aiAgent is not null)
        await Start(aiAgent);
}

static void ShowHelp()
{
    var helpText = @"# Fidus CLI Help

Available command line options:

- **-i, --inference-provider** `<string>`: Inference provider (e.g., huggingface, cerebras, google). Required.
- **-m, --model** `<string>`: Model name to use. Required.
- **-a, --apiToken** `<string>`: API token for authentication. Required.
- **-t, --temperature** `<decimal>`: Sampling temperature (e.g., 0.7). Optional.
- **-p, --topP** `<decimal>`: Nucleus sampling probability (e.g., 0.9). Optional.
- **-h, --help**: Show this help message and exit.

Examples:
    fidus -i huggingface -m gpt2 -a <token>
    fidus --inference-provider cerebras --model llama2 --apiToken <token> --temperature 0.7 --topP 0.9
";
    Console.WriteLine(MarkdownFormatter.FormatDocument(helpText));
}

static AIClient BuildAIAgent(Settings settings)
{
    try
    {
        if (settings is null)
            throw new Exception("AI Agent settings not valid");

        if (string.IsNullOrEmpty(settings.InferenceProvider))
            throw new Exception("Inference provider not set. Run command : fidus -i <inference_provider>");

        if (string.IsNullOrEmpty(settings.ModelName))
            throw new Exception("Model name not set. Run command : fidus -m <model_name>");

        if (string.IsNullOrEmpty(settings.ApiToken))
            throw new Exception("Api token not set. Run command : fidus -a <api_token>");


        AIClient aiClient = null;
        switch (settings.InferenceProvider)
        {
            case "huggingface":
                aiClient = PromptVitFactory.CreateHuggingFaceClient(settings.ApiToken, settings.ModelName);
                break;
            case "cerebras":
                aiClient = PromptVitFactory.CreateCerebrasClient(settings.ApiToken, settings.ModelName);
                break;
            case "google":
                aiClient = PromptVitFactory.CreateGoogleAIStudioClient(settings.ApiToken, settings.ModelName);
                break;
        }

        if (aiClient is null)
            throw new Exception("AI Agent inference provider not valid");

        aiClient.SetSystemPrompt(Agent.GetSystemPrompt());
        aiClient.SetTools(Agent.GetAgentTools());

        return aiClient;
    }
    catch (System.Exception ex)
    {
        Console.WriteLine("An error occured during AI Agent initialization: " + ex.Message);
        return null;
    }
}

static async Task Start(AIClient aiClient)
{

    Console.WriteLine(MarkdownFormatter.FormatDocument($"# Hello {Environment.UserName}, what can I do for you ?"));

    while (true)
    {
        Console.Write("> ");

        var userInput = Console.ReadLine();

        if (string.IsNullOrEmpty(userInput))
            break;

        Console.WriteLine();
        Console.WriteLine(MarkdownFormatter.FormatDocument("## Thinking..."));
        Console.WriteLine();

        var response = await aiClient.Invoke(userInput);

        Console.WriteLine(MarkdownFormatter.FormatDocument(response));
    }
}

