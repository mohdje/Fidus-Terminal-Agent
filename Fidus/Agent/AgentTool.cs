using System.Text.Json;
using Fidus.Utils;
using PromptVit;

namespace Fidus.Agent
{
    public abstract class AgentTool<T> : IAITool
    {
        protected readonly ConsoleHelper _consoleDrawer;
        public AgentTool(ConsoleHelper consoleDrawer)
        {
            _consoleDrawer = consoleDrawer;
        }
        public abstract string Name { get; }

        public abstract string Description { get; }
        public abstract string StatusMessage { get; }

        public abstract AIToolParameter[] Parameters { get; }

        public async Task<string> ExecuteToolAsync(string jsonParameters)
        {
            var parameters = DeserializeParameters<T>(jsonParameters);
            return await _consoleDrawer.RunWithStatusAsync(
                StatusMessage,
                async () => await ExecuteToolAsync(parameters), StatusSubMessage(parameters));
        }

        protected abstract string StatusSubMessage(T parameters);

        protected abstract Task<string> ExecuteToolAsync(T parameters);

        protected TParameter DeserializeParameters<TParameter>(string jsonParameters)
        {
            return JsonSerializer.Deserialize<TParameter>(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        }
    }
}