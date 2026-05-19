using Fidus;

public static class CommandArgsExtension
{
    readonly static CommandArg[] ValidCommandArgs =
    [
        new CommandArg(["-i", "--inference-provider"], typeof(string), "Inference provider cannot be an empty string"),
        new CommandArg(["-m", "--model"], typeof(string), "Model name cannot be an empty string"),
        new CommandArg(["-a", "--apiToken"], typeof(string), "Api token cannot be an empty string"),
        new CommandArg(["-t", "--temperature"], typeof(decimal), "Temperature must be a decimal value"),
        new CommandArg(["-p", "--topP"], typeof(decimal), "TopP must be a decimal value"),
    ];

    public static bool HasReadOnlyArgs(this string[]? commandArgs)
    {
        if (commandArgs == null)
            return false;

        if (commandArgs.Any(arg => arg == "-h" || arg == "--help"))
        {
            ShowHelp();
            return true;
        }
        if (commandArgs.Any(arg => arg == "-v" || arg == "--version"))
        {
            Console.WriteLine("Fidus CLI version 1.0.0");
            return true;
        }
        if (commandArgs.Any(arg => arg == "-l" || arg == "--logs"))
        {
            if (File.Exists("error.log"))
            {
                var logs = File.ReadAllText("error.log");
                Console.WriteLine(logs);
            }
            else
            {
                Console.WriteLine("No logs found.");
            }
            return true;
        }
        return false;
    }

    public static bool HasValidCommandArgs(this string[]? commandArgs)
    {
        var validArgs = ValidCommandArgs.SelectMany(c => c.Names);
        return commandArgs?.Any(arg => validArgs.Contains(arg)) == true;
    }

    private static void ShowHelp()
    {
        var helpText = @"# Fidus CLI Help

Available command line options:

- **-i, --inference-provider** `<string>`: Inference provider (e.g., huggingface, cerebras, google). Required.
- **-m, --model** `<string>`: Model name to use. Required.
- **-a, --apiToken** `<string>`: API token for authentication. Required.
- **-t, --temperature** `<decimal>`: Sampling temperature (e.g., 0.7). Optional.
- **-p, --topP** `<decimal>`: Nucleus sampling probability (e.g., 0.9). Optional.
- **-h, --help**: Show this help message and exit.
- **-v, --version**: Show the version of the application and exit.
- **-s, --settings**: Show the current settings and exit.

Examples:
    fidus -i huggingface -m gpt2 -a <token>
    fidus --inference-provider cerebras --model llama2 --apiToken <token> --temperature 0.7 --topP 0.9
";
        Console.WriteLine(MarkdownFormatter.FormatDocument(helpText));
    }

    public static Settings ReadArgs(this string[] commandArgs)
    {
        var settings = new Settings();
        for (int i = 0; i < ValidCommandArgs.Length; i++)
        {
            var arg = ValidCommandArgs[i];
            if (commandArgs.Any(c => arg.Names.Contains(c)))
            {
                var index = arg.Names.Select(n => commandArgs.IndexOf(n)).First(i => i > -1);
                var value = commandArgs.ElementAt(index + 1);
                decimal decimalValue = 0;

                if (string.IsNullOrEmpty(value))
                    Console.WriteLine(arg.InvalidMessage);

                if (arg.ValueType == typeof(decimal) && !decimal.TryParse(value, out decimalValue))
                    Console.WriteLine(arg.InvalidMessage);

                if (i == 0)
                {
                    settings.InferenceProvider = value;
                    Console.WriteLine("Inference provider updated");
                }
                else if (i == 1)
                {
                    settings.ModelName = value;
                    Console.WriteLine("Model name updated");
                }
                else if (i == 2)
                {
                    settings.ApiToken = value;
                    Console.WriteLine("Api token updated");
                }
                else if (i == 3)
                {
                    settings.Temperature = decimalValue;
                    Console.WriteLine("Temperature updated");
                }
                else if (i == 4)
                {
                    settings.TopP = decimalValue;
                    Console.WriteLine("TopP updated");
                }
            }
        }
        return settings;
    }
}