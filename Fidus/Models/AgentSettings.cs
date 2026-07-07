using Fidus.Enums;

namespace Fidus.Models
{
    public class AgentSettings
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public InferenceProvider? InferenceProvider { get; set; }
        public string? ApiToken { get; set; }
        public string? ModelName { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? TopP { get; set; }
    }
}
