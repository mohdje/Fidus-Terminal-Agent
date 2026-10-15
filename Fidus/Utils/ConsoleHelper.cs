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

        public ConsoleHelper() : this(AnsiConsole.Console) { }

        public ConsoleHelper(IAnsiConsole console) => _console = console;
        private List<TaskData> tasksList = [];

        class TaskData(int id, string message, string subMessage, DateTime startTime)
        {
            public int Id => id;
            public string Message => message;
            public string SubMessage => subMessage;
            public DateTime StartTime => startTime;
        }

        CancellationTokenSource cancellationTokenSource;
        public async Task<int> RenderTaskProcessingAsync(string message, string? subMessage = null)
        {
            cancellationTokenSource?.Cancel();

            await Task.Delay(100);

            var newTask = new TaskData(tasksList.Count + 1, message, subMessage, DateTime.Now);

            tasksList.Add(newTask);

            RenderTasksProcessing();

            return newTask.Id;
        }

        private void RenderTasksProcessing()
        {
            cancellationTokenSource = new CancellationTokenSource();

            var t = tasksList.LastOrDefault();
            if (t is null)
                return;

            var statusMessage = BuildStatusMessage(t.Message, subMessage: t.SubMessage);
            _console.Status()
              .Spinner(string.IsNullOrEmpty(t.SubMessage) ? Spinner.Known.Star : Spinner.Known.CircleHalves)
              .SpinnerStyle(Style.Parse("cyan"))
              .StartAsync(statusMessage, async ctx =>
              {
                  try
                  {
                      while (!cancellationTokenSource.IsCancellationRequested)
                      {
                          ctx.Status(BuildStatusMessage(t.Message, DateTime.Now - t.StartTime, t.SubMessage));
                          await Task.Delay(100, cancellationTokenSource.Token);
                      }
                  }
                  catch (TaskCanceledException) { }
              });

        }

        public async Task StopRenderTaskProcessingAsync(int taskId)
        {
            if (!tasksList.Any(t => t.Id == taskId))
                return;

            cancellationTokenSource?.Cancel();

            await Task.Delay(100);

            var taskToStop = tasksList.Single(t => t.Id == taskId);
            tasksList.Remove(taskToStop);

            _console.MarkupLine($":green_circle: {BuildStatusMessage(taskToStop.Message, DateTime.Now - taskToStop.StartTime, taskToStop.SubMessage)}");

            RenderTasksProcessing();
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

        public void RenderWelcomeScreen(string agentName)
        {
            DrawLogo();
            _console.WriteLine();
            _console.MarkupLine($"[magenta]Your[/] [cyan bold]{Markup.Escape(agentName)}[/] [magenta]assistant[/]");
            _console.WriteLine();
        }

        public async Task LoadChatHistoryAsync(Fidus.Agent.Agent agent)
        {
            var chatHistory = await agent.GetChatHistoryAsync();

            if (string.IsNullOrEmpty(chatHistory) is false)
            {
                RenderMarkdown(chatHistory);
                _console.WriteLine();
                _console.MarkupLine($"[italic gray]Previous session resumed for[/] [bold gray]{Markup.Escape(agent.Name)}[/] [italic gray]agent[/]");
            }
            else
            {
                _console.MarkupLine($"[italic gray]No previous session found for[/] [bold gray]{Markup.Escape(agent.Name)}[/] [italic gray]agent.[/]");
                _console.WriteLine();
                _console.MarkupLine($"[bold white]Hello [bold green]{Markup.Escape(Environment.UserName)}[/], what can I do for you?[/]");
            }
        }

        public void RenderNewSessionMessage(string agentName)
        {
            _console.MarkupLine($"[italic gray]New session for[/] [bold gray]{Markup.Escape(agentName)}[/] [italic gray]agent. To resume previous session run[/] [bold gray]fidus -r -a {agentName}[/]");
            _console.WriteLine();
            _console.MarkupLine($"[bold white]Hello [bold green]{Markup.Escape(Environment.UserName)}[/], what can I do for you?[/]");
        }

        public void RenderUpdateAvailable()
        {
            _console.MarkupLine("[green][italic]New version available. Run [/][bold]fidus --update[/][italic] to install it[/][/]");
            _console.WriteLine();
        }

        private void DrawLogo()
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


        private string BuildStatusMessage(string message, TimeSpan? ts = null, string subMessage = "")
        {
            string text = string.Empty;
            if (!string.IsNullOrEmpty(subMessage))
            {
                text = subMessage.Replace('\r', ' ').Replace('\n', ' ').Trim();
                if (text.Length > 120)
                    text = text[..117] + "...";
            }

            if (ts.HasValue)
                message += $"({FormatTimeSpan(ts.Value)})";

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
