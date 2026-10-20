namespace Fidus.Utils
{
    public static class AppFiles
    {
        private const string ApplicationDataFolderName = "Fidus";

        public static string DataDirectory => GetDataDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        public static string ErrorLogsFilePath => Path.Combine(DataDirectory, "error_logs");
        public static string AgentsFilePath => Path.Combine(DataDirectory, "agents.json");
        public static string GetAgentHistoryFile(int agentId) => Path.Combine(DataDirectory, "history", $"{agentId}.txt");
        public static string GetSystemPromptFile(int agentId) => Path.Combine(DataDirectory, "agents", $"{agentId}.md");
        public static string GetChatHistoryFile(int agentId) => Path.Combine(DataDirectory, "chats", $"{agentId}.md");

        public static string GetDataDirectory(string localApplicationDataDirectory)
        {
            if (string.IsNullOrWhiteSpace(localApplicationDataDirectory))
                throw new InvalidOperationException("The per-user application data directory could not be determined.");

            return Path.Combine(localApplicationDataDirectory, ApplicationDataFolderName);
        }

        public static void EnsureDataDirectories() => EnsureDataDirectories(DataDirectory);

        public static void EnsureDataDirectories(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(Path.Combine(dataDirectory, "agents"));
            Directory.CreateDirectory(Path.Combine(dataDirectory, "history"));
            Directory.CreateDirectory(Path.Combine(dataDirectory, "chats"));
        }

        public static void InitializeUserData()
        {
            var resourcesDirectory = Path.Combine(AppContext.BaseDirectory, "Resources");
            InitializeUserData(resourcesDirectory, DataDirectory);
        }

        public static void InitializeUserData(string resourcesDirectory, string dataDirectory)
        {
            EnsureDataDirectories(dataDirectory);

            CopyFileIfMissing(
                Path.Combine(resourcesDirectory, "agents.json"),
                Path.Combine(dataDirectory, "agents.json"));

            foreach (var directoryName in new[] { "agents", "history", "chats" })
            {
                CopyDirectoryFilesIfMissing(
                    Path.Combine(resourcesDirectory, directoryName),
                    Path.Combine(dataDirectory, directoryName));
            }

            if (!File.Exists(Path.Combine(dataDirectory, "agents.json")))
            {
                throw new FileNotFoundException(
                    $"Agent settings were not found in '{resourcesDirectory}' and could not be initialized.");
            }
        }

        private static void CopyDirectoryFilesIfMissing(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
                return;

            foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
                CopyFileIfMissing(sourceFile, Path.Combine(destinationDirectory, relativePath));
            }
        }

        private static void CopyFileIfMissing(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile) || File.Exists(destinationFile))
                return;

            var destinationDirectory = Path.GetDirectoryName(destinationFile)
                ?? throw new InvalidOperationException($"Could not determine the directory for '{destinationFile}'.");
            Directory.CreateDirectory(destinationDirectory);

            var temporaryFile = Path.Combine(
                destinationDirectory,
                $".{Path.GetFileName(destinationFile)}.{Guid.NewGuid():N}.tmp");

            try
            {
                File.Copy(sourceFile, temporaryFile);
                try
                {
                    File.Move(temporaryFile, destinationFile);
                }
                catch (IOException) when (File.Exists(destinationFile))
                {
                    // Another launch may have created the destination while this file was copied.
                }
            }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }
    }
}
