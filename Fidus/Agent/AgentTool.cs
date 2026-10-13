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

            var taskId = await _consoleDrawer.RenderTaskProcessingAsync(StatusMessage, StatusSubMessage(parameters));
            var result = await ExecuteToolAsync(parameters);
            await _consoleDrawer.StopRenderTaskProcessingAsync(taskId);
            return result;
        }

        protected abstract string StatusSubMessage(T parameters);

        protected abstract Task<string> ExecuteToolAsync(T parameters);

        protected TParameter DeserializeParameters<TParameter>(string jsonParameters)
        {
            return JsonSerializer.Deserialize<TParameter>(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        }
    }
}