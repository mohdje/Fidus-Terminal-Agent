using System.Text.Json;
using Fidus.Utils;
using PromptVit;

namespace Fidus.Agent
{
    public abstract class AgentTool<T> : IAITool
    {
        protected readonly ConsoleDrawer _consoleDrawer;
        public AgentTool(ConsoleDrawer consoleDrawer)
        {
            _consoleDrawer = consoleDrawer;
        }
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract AIToolParameter[] Parameters { get; }

        public Task<string> ExecuteToolAsync(string jsonParameters)
        {
            var parameters = DeserializeParameters<T>(jsonParameters);
            return ExecuteToolAsync(parameters);
        }

        protected abstract Task<string> ExecuteToolAsync(T parameters);

        protected T DeserializeParameters<T>(string jsonParameters)
        {
            return JsonSerializer.Deserialize<T>(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}