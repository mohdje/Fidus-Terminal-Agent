using Fidus.Enums;
using Fidus.Models;
using Fidus.Utils;
using PromptVit;
using PromptVit.AIClients;

namespace Fidus.Agent
{
    public class Agent
    {
        readonly AIClient aiClient;
        public readonly string Name;
        private readonly string chatHistoryFilePath;
        private Agent(AIClient aiClient, int agentId, string name)
        {
            this.aiClient = aiClient;
            Name = name;
            this.chatHistoryFilePath = AppFiles.GetChatHistoryFile(agentId);
        }

        public static async Task<Agent> CreateAsync(AgentSettings settings, bool loadHistory = false, IEnumerable<IAITool> aITools = null)
        {
            var aiClient = await BuildAIAgentAsync(settings, loadHistory, aITools);
            return new Agent(aiClient, settings.Id, settings.Name);
        }

        public async Task<string> Invoke(string userInput)
        {
            var response = await aiClient.Invoke(userInput);

            if (Directory.Exists(Path.GetDirectoryName(chatHistoryFilePath)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(chatHistoryFilePath));

            await aiClient.SaveHistoryAsync(chatHistoryFilePath);

            return response;
        }

        private static async Task<AIClient> BuildAIAgentAsync(AgentSettings settings, bool loadHistory, IEnumerable<IAITool> aITools)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings), "Agent settings cannot be null");

            if (!CheckSettings(settings, out string errorMessage))
            {
                errorMessage += $" To fix this issue run command : fidus --setup {settings.Name}";
                throw new Exception(errorMessage);
            }

            AIClient aiClient = null;
            switch (settings.InferenceProvider)
            {
                case InferenceProvider.OpenAI:
                    aiClient = PromptVitFactory.CreateOpenAIClient(settings.ApiToken, settings.ModelName);
                    break;
                case InferenceProvider.HuggingFace:
                    aiClient = PromptVitFactory.CreateHuggingFaceClient(settings.ApiToken, settings.ModelName);
                    break;
                case InferenceProvider.Cerebras:
                    aiClient = PromptVitFactory.CreateCerebrasClient(settings.ApiToken, settings.ModelName);
                    break;
                case InferenceProvider.GoogleAIStudio:
                    aiClient = PromptVitFactory.CreateGoogleAIStudioClient(settings.ApiToken, settings.ModelName);
                    break;
                case InferenceProvider.Groq:
                    aiClient = PromptVitFactory.CreateGroqClient(settings.ApiToken, settings.ModelName);
                    break;
            }

            if (aiClient is null)
                throw new Exception($"Error trying to create client for agent '{settings.Name}'");

            aiClient.Temperature = settings.Temperature!.Value;
            aiClient.TopP = settings.TopP!.Value;

            var loadSystemPrompt = true;
            var chatHistoryFilePath = AppFiles.GetChatHistoryFile(settings.Id);
            if (loadHistory)
            {
                if (File.Exists(chatHistoryFilePath))
                {
                    try
                    {
                        await aiClient.LoadHistoryAsync(chatHistoryFilePath);
                        loadSystemPrompt = false;
                    }
                    catch (Exception ex)
                    {
                        File.Delete(chatHistoryFilePath);
                    }
                }
            }

            if (loadSystemPrompt)
            {
                var systemPrompt = await GetSystemPromptAsync(settings);
                aiClient.SetSystemPrompt(systemPrompt);
            }

            if (aITools != null)
                aiClient.SetTools(aITools);

            return aiClient;
        }

        private static async Task<string> GetSystemPromptAsync(AgentSettings agentSettings)
        {
            var systemPromptFilePath = AppFiles.GetSystemPromptFile(agentSettings.Id);
            if (!File.Exists(systemPromptFilePath))
                return string.Empty;

            var systemPrompt = await File.ReadAllTextAsync(systemPromptFilePath);

            systemPrompt += Environment.NewLine + Environment.NewLine;
            systemPrompt += $@"# Environment context:
            - Operating System: {Environment.OSVersion}
            - User: {Environment.UserName}
            - Current Directory: {Environment.CurrentDirectory}
            - Current Date and Time: {DateTime.Now}";

            return systemPrompt;
        }

        private static bool CheckSettings(AgentSettings settings, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(settings.ModelName))
                errorMessage = $"Model name for agent '{settings.Name}' not set.";
            else if (string.IsNullOrEmpty(settings.ApiToken))
                errorMessage = $"Api token for agent '{settings.Name}' not set.";
            else if (!settings.InferenceProvider.HasValue)
                errorMessage = $"Inference provider for agent '{settings.Name}' not set.";
            else if (!settings.Temperature.HasValue || settings.Temperature < 0 || settings.Temperature > 2)
                errorMessage = $"Temperature for agent '{settings.Name}' must be between 0 and 2";
            else if (!settings.TopP.HasValue || settings.TopP < 0 || settings.TopP > 1)
                errorMessage = $"TopP for agent '{settings.Name}' must be between 0 and 1";

            return string.IsNullOrEmpty(errorMessage);
        }
    }
}