using System.Diagnostics;
using Spectre.Console;

namespace Fidus.Utils
{
    public class ConsoleHelper
    {
        bool loadingAnimationEnabled = false;
        int loadingRefreshRate = 200;
        Stopwatch stopwatch = new Stopwatch();
        string promptIndicator = "> ";

        CancellationTokenSource cancelAnimationTokenSource;

        public void DrawLogo()
        {
            var eye = "[white]◠[/]";
            var mouth = "[white]◡[/]";
            AnsiConsole.MarkupLine($"        [magenta]╭───────╮[/]");
            AnsiConsole.MarkupLine($"        [magenta]│[/]  {eye} {eye}  [magenta]│[/]");
            AnsiConsole.MarkupLine($"        [magenta]│[/]   {mouth}   [magenta]│[/]");
            AnsiConsole.MarkupLine($"        [magenta]╰───────╯[/]");
        }

        public async Task StartLoadingAnimationAsync(string message, string subMessage = "")
        {
            if (loadingAnimationEnabled)
            {
                await StopLoadingAnimationAsync();
            }

            stopwatch.Reset();
            stopwatch.Start();

            var thinkingAnimation = new string[] { "⣾", "⣷", "⣯", "⣟", "⣻", "⣽", "⣾" };
            int animationIndex = 0;

            Console.CursorVisible = false;

            loadingAnimationEnabled = true;

            var subMessageLength = 50;
            var displaySubmessage = subMessage.Length >= subMessageLength ? $"{subMessage[..subMessageLength]}..." : $"{subMessage}";

            Console.Write($"[cyan]{thinkingAnimation[animationIndex]}[/] [bold cyan]{message}[/] [bold brightblack]{displaySubmessage}[/]");

            cancelAnimationTokenSource = new CancellationTokenSource();
            while (loadingAnimationEnabled && !cancelAnimationTokenSource.Token.IsCancellationRequested)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"[cyan]{thinkingAnimation[animationIndex]}[/] ");
                animationIndex = animationIndex == thinkingAnimation.Length - 1 ? 0 : animationIndex + 1;
                try
                {
                    await Task.Delay(loadingRefreshRate, cancelAnimationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task StopLoadingAnimationAsync()
        {
            if (loadingAnimationEnabled)
            {
                loadingAnimationEnabled = false;
                cancelAnimationTokenSource.Cancel();
                try
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(char.ConvertFromUtf32(0x00002705));
                    Console.CursorLeft = Console.BufferWidth + 1;
                    Console.Write(" ");

                    stopwatch.Stop();

                    AnsiConsole.MarkupLine($"[brightblack]Done in {FormatTimeSpan(stopwatch.Elapsed)}[/]");
                    Console.WriteLine();

                    Console.CursorVisible = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public string GetUserPrompt()
        {
            var userInput = ReadLine.Read(promptIndicator);

            if (string.IsNullOrEmpty(userInput))
                return string.Empty;

            ReadLine.AddHistory(userInput);

            int totalLength = userInput.Length + promptIndicator.Length;
            int consoleWidth = Console.BufferWidth;

            int linesSpanned = (totalLength + consoleWidth - 1) / consoleWidth;
            int currentCursorTop = Console.CursorTop;
            for (int i = 0; i < linesSpanned; i++)
            {
                Console.SetCursorPosition(0, currentCursorTop - linesSpanned + i);
                Console.Write(new string(' ', consoleWidth - 1));
            }

            Console.SetCursorPosition(0, currentCursorTop - linesSpanned);
            AnsiConsole.MarkupLine($"[bold brightmagenta]{userInput}[/]");
            return userInput;
        }

        public int GetUserChoice(string prompt, string[] options, int? defaultChoiceIndex = null)
        {
            AnsiConsole.MarkupLine($"[bold brightmagenta]{prompt}[/]");
            for (int i = 0; i < options.Length; i++)
                AnsiConsole.MarkupLine($"[{i}] {options[i]}");

            if (defaultChoiceIndex.HasValue && defaultChoiceIndex.Value >= 0 && defaultChoiceIndex.Value < options.Length)
                AnsiConsole.MarkupLine($"[italic brightcyan]Press Enter to keep the default one: {options[defaultChoiceIndex.Value]}[/]");

            string? choiceIndex;
            bool notValidIndex;
            do
            {
                choiceIndex = ReadLine.Read(promptIndicator, defaultChoiceIndex.ToString());
                notValidIndex = !int.TryParse(choiceIndex, out int index) || index < 0 || index >= options.Length;
                if (notValidIndex)
                    AnsiConsole.MarkupLine($"[bold brightred]Invalid index. Please choose a valid index from the list above.[/]");

            } while (notValidIndex);
            return int.Parse(choiceIndex);
        }

        public string GetUserInput(string prompt, string defaultValue = "")
        {
            AnsiConsole.MarkupLine($"[bold brightmagenta]{prompt}[/]");
            if (!string.IsNullOrEmpty(defaultValue))
                AnsiConsole.MarkupLine($"[italic brightcyan]Press Enter to keep the default one: {defaultValue}[/]");

            var userInput = ReadLine.Read(promptIndicator, defaultValue);

            if (string.IsNullOrEmpty(userInput))
                return defaultValue;

            return userInput;
        }

        public decimal GetUserInput(string prompt, decimal min, decimal max, decimal? defaultValue = null)
        {
            AnsiConsole.MarkupLine($"[bold brightmagenta]{prompt}[/]");
            if (defaultValue.HasValue)
                AnsiConsole.MarkupLine($"[italic brightcyan]Press Enter to keep the default one: {defaultValue.Value}[/]");

            bool valueNotValid;
            decimal value;
            do
            {
                var valueInput = ReadLine.Read(promptIndicator, defaultValue.HasValue ? defaultValue.Value.ToString() : string.Empty);
                valueNotValid = !decimal.TryParse(valueInput, out value) || value < min || value > max;
                if (valueNotValid)
                    AnsiConsole.MarkupLine($"[bold brightred]Invalid value. Please enter a value between {min} and {max}.[/]");
            } while (valueNotValid);

            return value;
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
