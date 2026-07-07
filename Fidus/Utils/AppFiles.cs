namespace Fidus.Utils
{
    public static class AppFiles
    {
        public static string ErrorLogsFilePath => Path.Combine(AppContext.BaseDirectory, "error_logs");
        private static string ResourcesFolderPath => Path.Combine(AppContext.BaseDirectory, "Resources");
        public static string AgentsFilePath => Path.Combine(ResourcesFolderPath, "agents.json");
        public static string GetChatHistoryFile(int agentId) => Path.Combine(ResourcesFolderPath, "history", $"{agentId}.txt");
        public static string GetSystemPromptFile(int agentId) => Path.Combine(ResourcesFolderPath, "agents", $"{agentId}.md");
    }
}