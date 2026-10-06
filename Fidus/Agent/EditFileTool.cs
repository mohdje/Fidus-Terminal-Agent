
using Fidus.Utils;
using PromptVit;

namespace Fidus.Agent
{
    public class EditFileTool(ConsoleHelper consoleDrawer) : AgentTool<EditFileParameters>(consoleDrawer)
    {
        public override string Name => "editFile";

        public override string Description => "Edit a file.";

        public override AIToolParameter[] Parameters =>
            [
                new AIToolParameter("filePath", "The path to the file to edit. If does not exist, it will be created.", "string"),
                new AIToolParameter("content", "The new content for the file.", "string"),
            ];

        public override string StatusMessage => "Edit File";
        protected override string StatusSubMessage(EditFileParameters parameters)
        {
            return parameters.FilePath;
        }

        protected override async Task<string> ExecuteToolAsync(EditFileParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.FilePath))
                return "File path is required.";

            try
            {
                await File.WriteAllTextAsync(parameters.FilePath, parameters.Content);
                return $"File '{parameters.FilePath}' has been updated successfully.";
            }
            catch (Exception ex)
            {
                return $"Error updating file '{parameters.FilePath}': {ex.Message}";
            }
        }


    }
}