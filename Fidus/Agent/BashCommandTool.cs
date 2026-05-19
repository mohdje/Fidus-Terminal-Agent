using System.Diagnostics;
using System.Text;
using ConsoleInk;
using Fidus.Utils;
using PromptVit;

namespace Fidus.Agent
{
    public class BashCommandTool : AgentTool<ExecuteBashCommandParameters>
    {
        public BashCommandTool(ConsoleDrawer consoleDrawer) : base(consoleDrawer)
        {

        }
        public override string Name => "executeBashCommand";

        public override string Description => "Execute a bash command.";

        public override AIToolParameter[] Parameters =>
            [
                new AIToolParameter("bashCommand", "A bash command to execute (example: ls). Returns the output of the command.", "string"),
        ];


        protected override async Task<string> ExecuteToolAsync(ExecuteBashCommandParameters parameters)
        {
            var command = parameters.BashCommand;
            try
            {
                _consoleDrawer.StartLoadingAnimationAsync("Executing bash command", command);

                string tempScriptPath = null;
                bool isMultiLine = command.Contains("\n") || command.Contains("\r") || command.Contains("EOF");
                string arguments;

                if (isMultiLine)
                {
                    // Write the script to a temporary file
                    tempScriptPath = Path.Combine(Path.GetTempPath(), $"fidus_script_{Guid.NewGuid()}.sh");
                    await File.WriteAllTextAsync(tempScriptPath, command);
                    arguments = tempScriptPath;
                }
                else
                {
                    // Escape double quotes for shell safety
                    var safeCommand = command.Replace("\"", "\\\"");
                    arguments = $"-c \"{safeCommand}\"";
                }

                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });

                if (process == null)
                {
                    var message = $"Failed to start process for command: {command}";
                    Console.WriteLine(message);
                    if (tempScriptPath != null && File.Exists(tempScriptPath)) File.Delete(tempScriptPath);
                    return message;
                }

                var outputTask = StreamOutput(process.StandardOutput);
                var errorTask = StreamOutput(process.StandardError);

                var outputs = await Task.WhenAll(outputTask, errorTask);
                await process.WaitForExitAsync();

                if (tempScriptPath != null && File.Exists(tempScriptPath)) File.Delete(tempScriptPath);

                await _consoleDrawer.StopLoadingAnimationAsync();

                if (process.ExitCode == 0)
                {
                    return outputs[0];
                }
                else
                {
                    return outputs[1];
                }
            }
            catch (Exception ex)
            {
                await _consoleDrawer.StopLoadingAnimationAsync();
                Console.WriteLine($"ExecuteBashCommand failed: {ex.Message}");
                return $"ExecuteBashCommand failed: {ex.Message}";
            }
        }


        private Task<string> StreamOutput(StreamReader streamReader)
        {
            var output = new StringBuilder();
            while (true)
            {
                var line = streamReader.ReadLine();
                if (line != null)
                {
                    output.AppendLine(line);
                    _consoleDrawer.StartLoadingAnimationAsync("Executing bash command...", line);
                }
                else
                    break;
            }
            return Task.FromResult(output.ToString());
        }


    }
}