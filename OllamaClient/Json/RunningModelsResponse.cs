namespace OllamaClient.Json
{
    public record struct RunningModelsResponse
    {
        public RunningModelInfo[] models { get; set; }
    }
}