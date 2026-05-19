using ConsoleInk;

namespace Fidus.Utils
{
    public class ConsoleDrawer
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

            var subMessageLength = 20;
            var displaySubmessage = subMessage.Length >= subMessageLength ? $"{subMessage[..subMessageLength]}..." : $"{subMessage}";
            if (!string.IsNullOrEmpty(displaySubmessage))
                displaySubmessage = $"{displaySubmessage} ";

            Console.Write($"{Ansi.Bold}{Ansi.FgCyan}{message}{Ansi.Reset} {Ansi.Bold}{Ansi.FgBrightBlack}{displaySubmessage}{Ansi.Reset}");

            cancelAnimationTokenSource = new CancellationTokenSource();
            while (loadingAnimationEnabled && !cancelAnimationTokenSource.Token.IsCancellationRequested)
            {
                Console.Write($"{Ansi.FgCyan}{thinkingAnimation[animationIndex]}{Ansi.Reset}");
                animationIndex = animationIndex == thinkingAnimation.Length - 1 ? 0 : animationIndex + 1;
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
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
    }
}
