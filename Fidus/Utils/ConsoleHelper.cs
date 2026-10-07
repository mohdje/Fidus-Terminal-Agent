using System.Diagnostics;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using Spectre.Console;

namespace Fidus.Utils
{
    public class ConsoleHelper
    {
        private readonly IAnsiConsole _console;
        private readonly string _promptIndicator = "> ";
        private readonly MarkdownRenderer renderer = new();

        private StatusContext? statusContext = null;
        private readonly List<Tuple<string, string>> statusMessages = [];

        public ConsoleHelper() : this(AnsiConsole.Console) { }

        public ConsoleHelper(IAnsiConsole console) => _console = console;

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
                            _console.MarkupLine($":green_circle: {msg} [gray]{subMsg}[/]");

                        _console.WriteLine();
                    }

                    _console.MarkupLine($"[italic gray]{message} done in {FormatTimeSpan(stopwatch.Elapsed)}[/] \n");
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


        public void RenderWelcomeScreen(string agentName, bool loadHistory)
        {
            DrawLogo();
            _console.WriteLine();

            _console.MarkupLine($"[bold magenta]Your {Markup.Escape(agentName)} assistant[/]");

            if (loadHistory)
                _console.MarkupLine($"[italic gray]Resuming previous session for {Markup.Escape(agentName)}[/]");
            else
                _console.MarkupLine($"[italic gray]Starting a new session for {Markup.Escape(agentName)}[/]");

            _console.WriteLine();
            _console.MarkupLine($"[bold white]Hello [bold cyan]{Markup.Escape(Environment.UserName)}[/], what can I do for you?[/]");
        }

        public void DrawLogo()
        {
            var eye = "[white]◠[/]";
            var mouth = "[white]◡[/]";
            AnsiConsole.MarkupLine($"        [magenta]╭───────╮[/]");
            AnsiConsole.MarkupLine($"        [magenta]│[/]  {eye} {eye}  [magenta]│[/]");
            AnsiConsole.MarkupLine($"        [magenta]│[/]   {mouth}   [magenta]│[/]");
            AnsiConsole.MarkupLine($"        [magenta]╰───────╯[/]");
        }

        public void RenderMarkdown(string markdownContent)
        {
            var options = new SpectreDisplayOptions();
            var codeStyle = new Style(
                foreground: Color.Cyan,
                background: Color.Default,
                decoration: Decoration.Bold);

            options.CodeInLine = codeStyle;
            options.CodeBlock = codeStyle;
            options.HtmlInline = new Style(
                foreground: Color.Yellow,
                background: Color.Default,
                decoration: Decoration.Bold);


            options.Header = new SpectreTextStyle(foreground: Color.Magenta, decoration: Decoration.Bold);

            options.Headers[0] = new SpectreTextStyle(foreground: Color.BlueViolet, decoration: Decoration.Underline | Decoration.Bold);


            var result = renderer.Render(markdownContent, options);
            _console.Write(result.Root ?? Text.Empty);
        }

        public void RenderError(string errorMessage)
        {
            _console.MarkupLine($"[bold red]Error: {Markup.Escape(errorMessage)}[/]");
        }

        private string BuildStatusMessage(string message, string subMessage = "")
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

        private string FormatTimeSpan(TimeSpan ts)
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
