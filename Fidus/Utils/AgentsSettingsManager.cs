using System.Text.Json;
using Fidus.Models;

namespace Fidus.Utils
{
    public class AgentsSettingsManager
    {
        private List<AgentSettings> agentsSettings;

        public AgentsSettingsManager()
        {
            ReadSettings();
        }
        private void ReadSettings()
        {
            if (!File.Exists(AppFiles.AgentsFilePath))
                throw new FileNotFoundException($"{AppFiles.AgentsFilePath} not found");

            var settingsJson = File.ReadAllText(AppFiles.AgentsFilePath);
            agentsSettings = JsonSerializer.Deserialize<List<AgentSettings>>(settingsJson) ?? new List<AgentSettings>();
        }

        public AgentSettings CreateAgentSettings(string name)
        {
            var newAgentSettings = new AgentSettings
            {
                Name = name,
                Id = agentsSettings.Count
            };

            agentsSettings.Add(newAgentSettings);
            return newAgentSettings;
        }

        public AgentSettings GetAgentSettings(string name)
        {
            return agentsSettings.FirstOrDefault(a => a.Name == name);
        }

        public string[] GetAllAgentNames()
        {
            return agentsSettings.Select(a => a.Name).ToArray();
        }

        public void RemoveAgentSettings(string name)
        {
            var agentToRemove = agentsSettings.FirstOrDefault(a => a.Name == name);
            if (agentToRemove != null)
            {
                agentsSettings.Remove(agentToRemove);
            }
        }

        public void SaveSettings()
        {
            var settingsJson = JsonSerializer.Serialize(agentsSettings);
            File.WriteAllText(AppFiles.AgentsFilePath, settingsJson);
        }
    }
}