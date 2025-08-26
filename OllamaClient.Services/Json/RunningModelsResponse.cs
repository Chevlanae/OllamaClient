namespace OllamaClient.Services.Json
{
    public record struct RunningModelsResponse
    {
        public RunningModelInfo[] models { get; set; }
    }
}