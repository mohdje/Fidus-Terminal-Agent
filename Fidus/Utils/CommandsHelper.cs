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
    public static bool HasValidCommandArgs(this string[]? commandArgs)
    {
        var validArgs = ValidCommandArgs.SelectMany(c => c.Names);
        return commandArgs?.Any(arg => validArgs.Contains(arg)) == true;
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