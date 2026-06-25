using Fidus.Utils;
using PromptVit;
using PromptVit.AIClients;

namespace Fidus.Agent
{
    public class Agent
    {
        readonly AIClient _aiClient;
        readonly BashCommandTool bashCommandTool;
        readonly InternetSearchTool internetSearchTool;

        public Agent(Settings settings, ConsoleHelper consoleDrawer)
        {
            bashCommandTool = new BashCommandTool(consoleDrawer);
            internetSearchTool = new InternetSearchTool(consoleDrawer);
            _aiClient = BuildAIAgent(settings);
        }

        public async Task<string> Invoke(string userInput)
        {
            return await _aiClient.Invoke(userInput);
        }
        private AIClient BuildAIAgent(Settings settings)
        {
            if (settings is null)
                throw new Exception("AI Agent settings not valid");

            if (string.IsNullOrEmpty(settings.InferenceProvider))
                throw new Exception("Inference provider not set. Run command : fidus -i <inference_provider>");

            if (string.IsNullOrEmpty(settings.ModelName))
                throw new Exception("Model name not set. Run command : fidus -m <model_name>");

            if (string.IsNullOrEmpty(settings.ApiToken))
                throw new Exception("Api token not set. Run command : fidus -a <api_token>");

            if (settings.Temperature.HasValue && (settings.Temperature < 0 || settings.Temperature > 2))
                throw new Exception("Temperature must be between 0 and 2");

            if (settings.TopP.HasValue && (settings.TopP < 0 || settings.TopP > 1))
                throw new Exception("TopP must be between 0 and 1");

            AIClient aiClient = null;
            switch (settings.InferenceProvider)
            {
                case "openai":
                    aiClient = PromptVitFactory.CreateOpenAIClient(settings.ApiToken, settings.ModelName);
                    break;
                case "huggingface":
                    aiClient = PromptVitFactory.CreateHuggingFaceClient(settings.ApiToken, settings.ModelName);
                    break;
                case "cerebras":
                    aiClient = PromptVitFactory.CreateCerebrasClient(settings.ApiToken, settings.ModelName);
                    break;
                case "google":
                    aiClient = PromptVitFactory.CreateGoogleAIStudioClient(settings.ApiToken, settings.ModelName);
                    break;
                case "groq":
                    aiClient = PromptVitFactory.CreateGroqClient(settings.ApiToken, settings.ModelName);
                    break;
            }

            if (aiClient is null)
                throw new Exception("AI Agent inference provider not valid");

            if (settings.Temperature.HasValue)
                aiClient.Temperature = settings.Temperature.Value;

            if (settings.TopP.HasValue)
                aiClient.TopP = settings.TopP.Value;

            aiClient.SetSystemPrompt(GetSystemPrompt());
            aiClient.SetTools([bashCommandTool, internetSearchTool]);

            return aiClient;
        }

        private string GetSystemPrompt()
        {
            return @$"You are an extremely capable CLI assistant that lives inside the user's terminal.

            You have two equally important modes:

            1. Shell executor mode — when the user wants to DO something on their computer (create/move/delete files, run programs, query system state, install packages, git operations, start servers, search files, process data with awk/sed/jq, etc.)
            2. Normal helpful assistant — answer questions, explain concepts, write short code snippets, give advice, debug problems conceptually, etc.

            Rules you MUST follow strictly:
            - When user asks a question about recent events, use the internet search tool internetSearch to get up-to-date information and include it in your answer. DO NOT make up information that can be easily searched on the web.
            - DO NOT use executeBashCommand tool to do search on the internet like running curl or similar commands to get the information. Use the internet search tool provided.
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
            - When you answer a user question, DO NOT use tables as it is not terminal friendly. Prefer a clear list unless asked by user.
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
    }
}