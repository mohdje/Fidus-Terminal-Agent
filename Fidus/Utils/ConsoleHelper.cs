using ConsoleInk;

namespace Fidus.Utils
{
    public class ConsoleHelper
    {
        bool loadingAnimationEnabled = false;
        int loadingRefreshRate = 200;

        CancellationTokenSource cancelAnimationTokenSource;

        public void DrawLogo()
        {
            Console.WriteLine($"    {Ansi.BgMagenta}{"                      "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{"                        "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{Ansi.FgBrightWhite}{"      ██        ██      "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{Ansi.FgBrightWhite}{"     █  █      █  █     "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{"                        "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{Ansi.FgBrightWhite}{"        █      █        "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{Ansi.FgBrightWhite}{"         ██████         "}{Ansi.Reset}");
            Console.WriteLine($"   {Ansi.BgMagenta}{"                        "}{Ansi.Reset}");
            Console.WriteLine($"    {Ansi.BgMagenta}{"                      "}{Ansi.Reset}");
        }

        public async Task StartLoadingAnimationAsync(string message, string subMessage = "")
        {
            if (loadingAnimationEnabled)
            {
                await StopLoadingAnimationAsync();
            }

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
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.CursorVisible = true;
            }
        }

        public string GetUserInput()
        {
            var promptIndicator = "> ";
            Console.Write(promptIndicator);
            var userInput = Console.ReadLine();

            if (string.IsNullOrEmpty(userInput))
                return string.Empty;

            int totalLength = userInput.Length + promptIndicator.Length;
            int consoleWidth = Console.BufferWidth;

            // Calculate how many rows this input spans (ceiling division)
            int linesSpanned = (totalLength + consoleWidth - 1) / consoleWidth;

            // Move cursor to start of the input line and clear all spanned lines
            int currentCursorTop = Console.CursorTop;
            for (int i = 0; i < linesSpanned; i++)
            {
                Console.SetCursorPosition(0, currentCursorTop - linesSpanned + i);
                Console.Write(new string(' ', consoleWidth - 1)); // Clear the line
            }

            // Reposition cursor and rewrite the input formatted
            Console.SetCursorPosition(0, currentCursorTop - linesSpanned);
            Console.WriteLine($"> {Ansi.FgBrightMagenta}{userInput}{Ansi.Reset}");
            return userInput;
        }
    }
}
