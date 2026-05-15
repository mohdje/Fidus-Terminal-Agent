using ConsoleInk;

namespace Fidus.Utils
{
    public class ConsoleDrawer
    {
        bool loadingAnimationEnabled = false;
        int loadingRefreshRate = 200;

        int cursorMessageTop;
        int? cursorSubmessageTop;


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
            Console.Write($"{Ansi.Bold}{Ansi.FgCyan}{message}{Ansi.Reset}");

            int left = Console.CursorLeft;
            cursorMessageTop = Console.CursorTop;

            if (!string.IsNullOrEmpty(subMessage))
            {
                Console.WriteLine();
                Console.Write($"{Ansi.Bold}{Ansi.FgBrightBlack}{subMessage}{Ansi.Reset}");
                cursorMessageTop -= 1;
                cursorSubmessageTop = Console.CursorTop;
            }

            while (loadingAnimationEnabled)
            {
                Console.SetCursorPosition(left, cursorMessageTop);
                Console.Write($"{Ansi.Bold}{Ansi.FgCyan}{thinkingAnimation[animationIndex]}{Ansi.Reset}");
                animationIndex = animationIndex == thinkingAnimation.Length - 1 ? 0 : animationIndex + 1;
                await Task.Delay(loadingRefreshRate);
            }
        }

        public async Task StopLoadingAnimationAsync()
        {
            if (loadingAnimationEnabled)
            {
                loadingAnimationEnabled = false;
                await Task.Delay(loadingRefreshRate * 2);

                EraseLine(cursorMessageTop);
                if (cursorSubmessageTop.HasValue)
                    EraseLine(cursorSubmessageTop.Value);

                Console.CursorVisible = true;
            }
        }

        private void EraseLine(int line)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line);
        }
    }
}
