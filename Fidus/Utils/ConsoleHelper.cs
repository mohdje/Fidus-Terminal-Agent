using System.Diagnostics;
using Spectre.Console;

namespace Fidus.Utils
{
    public class ConsoleHelper
    {
        private readonly IAnsiConsole _console;
        private readonly string _promptIndicator = "> ";

        public ConsoleHelper() : this(AnsiConsole.Console) { }

        public ConsoleHelper(IAnsiConsole console) => _console = console;

        public void DrawLogo()
        {
            _console.Write(new FigletText("FIDUS").Centered().Color(Color.DarkMagenta));
            _console.WriteLine();
        }

        StatusContext statusContext;
        List<Tuple<string, string>> statusMessages = new List<Tuple<string, string>>();
        public async Task<T> RunWithStatusAsync<T>(string message, Func<Task<T>> action, string? subMessage = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var statusMessage = BuildStatusMessage(message, subMessage);
                if (statusContext is not null)
                {
                    var previousStatusMessage = statusContext.Status;
                    statusContext.Status(statusMessage).Spinner(Spinner.Known.CircleHalves);
                    statusContext.Refresh();
                    var result = await action();
                    statusContext.Status(previousStatusMessage).Spinner(Spinner.Known.Star);

                    return result;
                }

                return await _console.Status()
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("cyan"))
                    .StartAsync(statusMessage, async ctx =>
                    {
                        statusContext = ctx;
                        var result = await action();
                        statusContext = null;
                        return result;
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while executing the action: {ex.Message}", ex);
            }
            finally
            {
                stopwatch.Stop();
                if (statusContext is null)
                {
                    if (statusMessages.Count > 0)
                    {
                        foreach (var (msg, subMsg) in statusMessages)
                            AnsiConsole.MarkupLine($":green_circle: {msg} [gray]{subMsg}[/]");

                        AnsiConsole.WriteLine();
                    }

                    AnsiConsole.MarkupLine($"[italic gray]{message} done in {FormatTimeSpan(stopwatch.Elapsed)}[/] \n");
                    statusMessages.Clear();
                }
                else
                    statusMessages.Add(new Tuple<string, string>($"{message} ({FormatTimeSpan(stopwatch.Elapsed)})", subMessage ?? string.Empty));
            }
        }

        public string GetUserPrompt()
        {
            var input = ReadLine.Read(_promptIndicator);
            if (!string.IsNullOrEmpty(input))
                ReadLine.AddHistory(input);
            return input ?? string.Empty;
        }

        public int GetUserChoice(string prompt, string[] options)
        {
            var selection = new SelectionPrompt<string>()
                .Title(prompt)
                .AddChoices(options);

            var choice = _console.Prompt(selection);
            return Array.IndexOf(options, choice);
        }

        public string GetUserInput(string prompt, string? defaultValue = null)
        {
            var textPrompt = new TextPrompt<string>(prompt)
                .AllowEmpty();

            if (!string.IsNullOrEmpty(defaultValue))
                textPrompt.DefaultValue(defaultValue);

            return _console.Prompt(textPrompt);
        }

        public string GetSecretInput(string prompt, string? existingValue = null)
        {
            var textPrompt = new TextPrompt<string>(prompt)
                .Secret()
                .AllowEmpty();

            var result = _console.Prompt(textPrompt);
            return string.IsNullOrEmpty(result) ? existingValue : result;
        }

        public decimal GetUserDecimal(string prompt, decimal min, decimal max, decimal? defaultValue = null)
        {
            var decimalPrompt = new TextPrompt<decimal>(prompt)
                .Validate(val => val >= min && val <= max
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be between {min} and {max}"));

            if (defaultValue.HasValue)
                decimalPrompt.DefaultValue(defaultValue.Value);

            return _console.Prompt(decimalPrompt);
        }

        private static string BuildStatusMessage(string message, string subMessage = "")
        {
            string text = string.Empty;
            if (!string.IsNullOrEmpty(subMessage))
            {
                text = subMessage.Replace('\r', ' ').Replace('\n', ' ').Trim();
                if (text.Length > 120)
                    text = text[..117] + "...";
            }

            return $"{message} [gray]{text}[/]".Trim();
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return ts.ToString(@"h\:mm\:ss") + " (h:min:sec)";
            if (ts.TotalMinutes >= 1)
                return ts.ToString(@"m\:ss") + "min";
            if (ts.TotalSeconds < 1)
                return $"{ts.TotalMilliseconds / 1000:F2}s";
            return $"{(int)ts.TotalSeconds}sec";
        }
    }
}
