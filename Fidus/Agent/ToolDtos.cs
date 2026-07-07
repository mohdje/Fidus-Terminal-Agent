namespace Fidus.Agent
{
    public class ExecuteBashCommandParameters
    {
        public string BashCommand { get; set; }
    }

    public class InternetSearchParameters
    {
        public string Query { get; set; }
    }

    public class InternetSearchResults
    {
        public InternetSearchResult[] Results { get; set; }
    }

    public class InternetSearchResult
    {
        public string Content { get; set; }
        public string Url { get; set; }
    }

    public class EditFileParameters
    {
        public string FilePath { get; set; }
        public string Content { get; set; }
    }

    public class ReadFileParameters
    {
        public string FilePath { get; set; }
    }
}