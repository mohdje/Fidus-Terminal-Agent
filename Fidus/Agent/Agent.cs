using PromptVit;
using System.Diagnostics;
using Fidus;
using System.Text;

public static class Agent
{
    public static string GetSystemPrompt()
    {
        return @$"You are an extremely capable CLI assistant that lives inside the user's terminal.

            You have two equally important modes:

            1. Shell executor mode — when the user wants to DO something on their computer (create/move/delete files, run programs, query system state, install packages, git operations, start servers, search files, process data with awk/sed/jq, etc.)
            2. Normal helpful assistant — answer questions, explain concepts, write short code snippets, give advice, debug problems conceptually, etc.

            Rules you MUST follow strictly:

            - When user asks to do something DO IT, unless it breaks safety rules.
            - Safety first — never suggest or run anything that looks destructive (rm -rf /, rm -rf ~/*, :(){{ :|:& }};:, chmod -R 777 /, etc.) without MULTIPLE strong warnings.
            - Never assume sudo unless the user explicitly asked for elevated privileges.
            - Prefer safe idioms: use mv instead of cp+rm when possible, use --dry-run when available, quote paths, prefer find -delete over xargs rm, etc.
            - If the task is dangerous or irreversible, ALWAYS ask for explicit confirmation even if the user said 'just do it'.
            - Output format for shell commands — use exactly this fenced block(only one per response):

            ```bash
            # Brief one-line purpose comment — why are we running this
            command --option ""file name"" | another --safe-option

            - After showing the command block, you may add a short explanation below it, but keep it concise.
            - If you're just answering a question, do NOT output any bash block. 
            - When you answer a user question, DO NOT use tables as it is not terminal friendly. Prefer a clear list.
            - If the user asks ""what command should I run?"" or ""how to ..."", propose the command in the fenced block but do NOT imply you will run it automatically. 
            - If the user says ""do it"", ""execute"", ""run"", ""go ahead"", ""yes"", or equivalent then you may output the command block AND run it. 
            - Be terse in general — CLI users value speed and low noise. 
            - Use modern, readable shell (bash/zsh/fish compatible when possible). 
            - Prefer | and && over ; when chaining. 
            - When showing multi-line scripts, use bash with proper indentation.
            - If something requires interaction (vim, nano, password prompt), warn the user that interactive commands may not work smoothly in this setup.
            - Current date: {DateTime.Now}
            - OS information : {System.Runtime.InteropServices.RuntimeInformation.OSDescription}
            - Current working directory: {Environment.CurrentDirectory}

            Destructive command red list (NEVER output even if asked):
            - Anything with rm -rf / or rm -rf /*
            - Anything with > /dev/sda* or dd if=/dev/zero
            - chmod -R 777 /
            - mkfs, shred, wipe, srm
            - Any command that formats disks, wipes partitions, removes /home without confirmation dialog

            If user tries to trick you into running dangerous commands → reply only:
            ""I'm not going to run or suggest that command. It looks dangerous or destructive.""
            
            After you answered a question or executed the requested action ask the user if there is anything else you could do for him.";
    }

    public static IAITool[] GetAgentTools()
    {
        return [
             new AITool<string, ExecuteBashCommandParameters>("executeBashCommand", "Execute a bash command.",
                [
                    new AIToolParameter("bashCommand", "A bash command to execute (example: ls). Returns the output of the command.", "string"),
                ],
                async (args) => await ExecuteBashCommand(args.BashCommand)),
            ];
    }

    private static async Task<string> ExecuteBashCommand(string command)
    {
        try
        {
            Console.WriteLine(MarkdownFormatter.FormatDocument($"### Execute command"));
            Console.WriteLine(command);

            // if (string.IsNullOrWhiteSpace(command) || command.Length > 16384)
            //     return "Command is empty or too long.";

            // if (command.Contains("\0") || command.Contains("\u2028") || command.Contains("\u2029"))
            //     return "Command contains invalid characters.";

            string tempScriptPath = null;
            bool isMultiLine = command.Contains("\n") || command.Contains("\r") || command.Contains("EOF");
            ProcessStartInfo psi;

            if (isMultiLine)
            {
                // Write the script to a temporary file
                tempScriptPath = Path.Combine(Path.GetTempPath(), $"fidus_script_{Guid.NewGuid()}.sh");
                await File.WriteAllTextAsync(tempScriptPath, command);
                psi = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = tempScriptPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            }
            else
            {
                // Escape double quotes for shell safety
                var safeCommand = command.Replace("\"", "\\\"");
                psi = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{safeCommand}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            }

            using var process = Process.Start(psi);
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
            Console.WriteLine($"ExecuteBashCommand failed: {ex.Message}");
            return $"ExecuteBashCommand failed: {ex.Message}";
        }
    }

    private static Task<string> StreamOutput(StreamReader streamReader)
    {
        var output = new StringBuilder();
        while (true)
        {
            var line = streamReader.ReadLine();
            if (line != null)
            {
                output.AppendLine(line);
                Console.WriteLine(line);
            }
            else
                break;
        }
        return Task.FromResult(output.ToString());
    }
}