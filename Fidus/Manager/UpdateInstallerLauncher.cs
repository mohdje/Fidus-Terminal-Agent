using System.Diagnostics;
using System.Text;
using Fidus.Enums;

namespace Fidus.Manager
{
    public interface IUpdateInstallerLauncher
    {
        void Start(InstallerPlatform platform, string scriptPath, string temporaryDirectory);
    }

    public sealed class UpdateInstallerLauncher : IUpdateInstallerLauncher
    {
        public void Start(InstallerPlatform platform, string scriptPath, string temporaryDirectory)
        {
            var wrapperPath = Path.Combine(temporaryDirectory, platform == InstallerPlatform.Windows ? "run-installer.ps1" : "run-installer.sh");
            var startInfo = CreateStartInfo(platform, scriptPath, wrapperPath, temporaryDirectory);

            File.WriteAllText(wrapperPath, GetWrapperContent(platform));

            using var process = Process.Start(startInfo);
            if (process is null)
                throw new InvalidOperationException("The installer process could not be started.");

            if (platform != InstallerPlatform.Windows)
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"The elevated installer launcher exited with code {process.ExitCode}.");
            }
        }

        public static ProcessStartInfo CreateStartInfo(
            InstallerPlatform platform,
            string scriptPath,
            string wrapperPath,
            string temporaryDirectory)
        {
            var startInfo = platform switch
            {
                InstallerPlatform.Windows => new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = true,
                    Verb = "runas"
                },
                InstallerPlatform.Linux => new ProcessStartInfo
                {
                    FileName = "sudo",
                    UseShellExecute = false
                },
                InstallerPlatform.MacOS => new ProcessStartInfo
                {
                    FileName = "osascript",
                    UseShellExecute = false
                },
                _ => throw new PlatformNotSupportedException($"Installer platform '{platform}' is not supported.")
            };

            if (platform == InstallerPlatform.Windows)
            {
                startInfo.Arguments = string.Join(" ", new[]
                {
                    "-NoProfile",
                    "-ExecutionPolicy",
                    "Bypass",
                    "-File",
                    wrapperPath,
                    scriptPath
                }.Select(QuoteWindowsArgument));
            }
            else if (platform == InstallerPlatform.Linux)
            {
                startInfo.ArgumentList.Add("-b");
                startInfo.ArgumentList.Add("--");
                startInfo.ArgumentList.Add("bash");
                startInfo.ArgumentList.Add(wrapperPath);
                startInfo.ArgumentList.Add(scriptPath);
            }
            else
            {
                var command = $"cd {QuoteShellArgument(temporaryDirectory)} && nohup /bin/bash {QuoteShellArgument(wrapperPath)} {QuoteShellArgument(scriptPath)} </dev/null >/dev/null 2>&1 &";
                startInfo.ArgumentList.Add("-e");
                startInfo.ArgumentList.Add($"do shell script \"{EscapeAppleScriptString(command)}\" with administrator privileges");
            }

            startInfo.WorkingDirectory = temporaryDirectory;
            return startInfo;
        }

        private static string QuoteWindowsArgument(string argument)
        {
            var quotedArgument = new StringBuilder("\"");
            var backslashes = 0;

            foreach (var character in argument)
            {
                if (character == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (character == '"')
                    quotedArgument.Append('\\', backslashes * 2 + 1);
                else
                    quotedArgument.Append('\\', backslashes);

                quotedArgument.Append(character);
                backslashes = 0;
            }

            quotedArgument.Append('\\', backslashes * 2);
            quotedArgument.Append('"');
            return quotedArgument.ToString();
        }

        private static string QuoteShellArgument(string argument) =>
            $"'{argument.Replace("'", "'\\''")}'";

        private static string EscapeAppleScriptString(string value) =>
            value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private string GetWrapperContent(InstallerPlatform platform) =>
            platform switch
            {
                InstallerPlatform.Windows => """
                    $ErrorActionPreference = 'Stop'
                    $scriptPath = $args[0]
                    $wrapperPath = $PSCommandPath
                    try {
                        & $scriptPath
                        if (-not $?) { exit 1 }
                    }
                    finally {
                        Remove-Item -LiteralPath $scriptPath -Force -ErrorAction SilentlyContinue
                        Remove-Item -LiteralPath $wrapperPath -Force -ErrorAction SilentlyContinue
                    }
                    """,
                InstallerPlatform.Linux or InstallerPlatform.MacOS => """
                    #!/usr/bin/env bash
                    script_path=$1
                    wrapper_path=$0
                    trap 'rm -f -- "$script_path" "$wrapper_path"' EXIT
                    bash "$script_path"
                    """,
                _ => throw new PlatformNotSupportedException($"Installer platform '{platform}' is not supported.")
            };
    }
}