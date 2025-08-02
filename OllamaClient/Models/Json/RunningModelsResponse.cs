namespace OllamaClient.Models.Json
{
    public record struct RunningModelsResponse
    {
        public RunningModelInfo[] models { get; set; }
    }
}