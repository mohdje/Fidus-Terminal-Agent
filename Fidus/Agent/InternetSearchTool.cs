using System.Text;
using System.Text.Json;
using Fidus.Utils;
using PromptVit;

namespace Fidus.Agent
{
    public class InternetSearchTool : AgentTool<InternetSearchParameters>
    {
        readonly HttpClient _httpClient;
        public override string Name => "internetSearch";

        public override string Description => "Perform an internet search using the provided query and return a list of relevant results. Use this tool to get up-to-date information from the web.";

        public override AIToolParameter[] Parameters =>
            [
                new AIToolParameter("query", "The search query to perform on the internet (example: 'What is weather today in Paris?').", "string"),
            ];

        public InternetSearchTool(ConsoleDrawer consoleDrawer) : base(consoleDrawer)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.tavily.com/search")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

        }
        protected override async Task<string> ExecuteToolAsync(InternetSearchParameters parameters)
        {
            _consoleDrawer.StartLoadingAnimationAsync("Searching on the internet...", parameters.Query);

            var queryObject = new { Query = parameters.Query, SearchDepth = "advanced" };
            var queryJson = JsonSerializer.Serialize(queryObject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var result = await _httpClient.PostAsync("", new StringContent(queryJson, Encoding.UTF8, "application/json"));

            await _consoleDrawer.StopLoadingAnimationAsync();

            if (!result.IsSuccessStatusCode)
                return $"Error performing internet search: {result.StatusCode}";

            var responseContent = await result.Content.ReadAsStringAsync();
            var searchResults = JsonSerializer.Deserialize<InternetSearchResults>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (searchResults?.Results == null || searchResults.Results.Length == 0)
                return "No results found for the query.";

            var formattedResults = searchResults.Results
                .Select((r, index) => $"Result {index + 1}:\nUrl: {r.Url}\nContent: {r.Content}\n")
                .Aggregate((a, b) => a + "\n" + b);
            return formattedResults;
        }
    }
}