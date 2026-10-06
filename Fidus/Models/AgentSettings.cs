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
        public string? MaskedApiToken => string.IsNullOrEmpty(ApiToken)
            ? null
            : ApiToken.Length <= 8
                ? new string('*', ApiToken.Length)
                : $"{ApiToken[..4]}****{ApiToken[^4..]}";
        public string? ModelName { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? TopP { get; set; }
    }
}
