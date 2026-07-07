using Fidus.Utils;
using PromptVit;

namespace Fidus.Agent
{
    public class ReadFileTool(ConsoleHelper consoleDrawer) : AgentTool<ReadFileParameters>(consoleDrawer)
    {
        public override string Name => "readFile";

        public override string Description => "Read the content of a file.";

        public override AIToolParameter[] Parameters =>
            [
                new AIToolParameter("filePath", "The path to the file to read.", "string"),
            ];

        protected override async Task<string> ExecuteToolAsync(ReadFileParameters parameters)
        {
            consoleDrawer.StartLoadingAnimationAsync($"Reading file: {parameters.FilePath}");

            if (string.IsNullOrEmpty(parameters.FilePath))
                return "File path is required.";

            try
            {
                if (!File.Exists(parameters.FilePath))
                    return $"File '{parameters.FilePath}' does not exist.";

                var content = await File.ReadAllTextAsync(parameters.FilePath);
                return content;
            }
            catch (Exception ex)
            {
                return $"Error reading file '{parameters.FilePath}': {ex.Message}";
            }
            finally
            {
                await consoleDrawer.StopLoadingAnimationAsync();
            }
        }
    }
}
