using System.Diagnostics;
using ConsoleInk;

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
            var eye = $"{Ansi.FgWhite}{"◠"}{Ansi.Reset}";
            var mouth = $"{Ansi.FgWhite}{"◡"}{Ansi.Reset}";
            Console.WriteLine($"        {Ansi.FgMagenta}{"╭───────╮"}{Ansi.Reset}");
            Console.WriteLine($"        {Ansi.FgMagenta}{"│"}{Ansi.Reset}  {eye} {eye}  {Ansi.FgMagenta}{"│"}{Ansi.Reset}");
            Console.WriteLine($"        {Ansi.FgMagenta}{"│"}{Ansi.Reset}   {mouth}   {Ansi.FgMagenta}{"│"}{Ansi.Reset}");
            Console.WriteLine($"        {Ansi.FgMagenta}{"╰───────╯"}{Ansi.Reset}");
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

            Console.Write($"{Ansi.FgCyan}{thinkingAnimation[animationIndex]}{Ansi.Reset} {Ansi.Bold}{Ansi.FgCyan}{message}{Ansi.Reset} {Ansi.Bold}{Ansi.FgBrightBlack}{displaySubmessage}{Ansi.Reset}");

            cancelAnimationTokenSource = new CancellationTokenSource();
            while (loadingAnimationEnabled && !cancelAnimationTokenSource.Token.IsCancellationRequested)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{Ansi.FgCyan}{thinkingAnimation[animationIndex]}{Ansi.Reset} ");
                animationIndex = animationIndex == thinkingAnimation.Length - 1 ? 0 : animationIndex + 1;
                try
                {
                    await Task.Delay(loadingRefreshRate, cancelAnimationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public async Task StopLoadingAnimationAsync()
        {
            if (loadingAnimationEnabled)
            {
                loadingAnimationEnabled = false;
                cancelAnimationTokenSource.Cancel();

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(char.ConvertFromUtf32(0x00002705));
                Console.CursorLeft = Console.BufferWidth + 1;
                Console.Write(" ");

                stopwatch.Stop();

                Console.WriteLine($"{Ansi.FgBrightBlack}Done in {FormatTimeSpan(stopwatch.Elapsed)}{Ansi.Reset}");
                Console.WriteLine();

                Console.CursorVisible = true;
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
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}{userInput}{Ansi.Reset}");
            return userInput;
        }

        public int GetUserChoice(string prompt, string[] options, int? defaultChoiceIndex = null)
        {
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}{prompt}{Ansi.Reset}");
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine($"[{i}] {options[i]}");

            if (defaultChoiceIndex.HasValue && defaultChoiceIndex.Value >= 0 && defaultChoiceIndex.Value < options.Length)
                Console.WriteLine($"{Ansi.Italic}{Ansi.FgBrightCyan}Press Enter to keep the default one: {options[defaultChoiceIndex.Value]}{Ansi.Reset}");

            string? choiceIndex;
            bool notValidIndex;
            do
            {
                choiceIndex = ReadLine.Read(promptIndicator, defaultChoiceIndex.ToString());
                notValidIndex = !int.TryParse(choiceIndex, out int index) || index < 0 || index >= options.Length;
                if (notValidIndex)
                    Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightRed}Invalid index. Please choose a valid index from the list above.{Ansi.Reset}");

            } while (notValidIndex);
            return int.Parse(choiceIndex);
        }

        public string GetUserInput(string prompt, string defaultValue = "")
        {
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}{prompt}{Ansi.Reset}");
            if (!string.IsNullOrEmpty(defaultValue))
                Console.WriteLine($"{Ansi.Italic}{Ansi.FgBrightCyan}Press Enter to keep the default one: {defaultValue}{Ansi.Reset}");

            var userInput = ReadLine.Read(promptIndicator, defaultValue);

            if (string.IsNullOrEmpty(userInput))
                return defaultValue;

            return userInput;
        }

        public decimal GetUserInput(string prompt, decimal min, decimal max, decimal? defaultValue = null)
        {
            Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightMagenta}{prompt}{Ansi.Reset}");
            if (defaultValue.HasValue)
                Console.WriteLine($"{Ansi.Italic}{Ansi.FgBrightCyan}Press Enter to keep the default one: {defaultValue.Value}{Ansi.Reset}");

            bool valueNotValid;
            decimal value;
            do
            {
                var valueInput = ReadLine.Read(promptIndicator, defaultValue.HasValue ? defaultValue.Value.ToString() : string.Empty);
                valueNotValid = !decimal.TryParse(valueInput, out value) || value < min || value > max;
                if (valueNotValid)
                    Console.WriteLine($"{Ansi.Bold}{Ansi.FgBrightRed}Invalid value. Please enter a value between {min} and {max}.{Ansi.Reset}");
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

