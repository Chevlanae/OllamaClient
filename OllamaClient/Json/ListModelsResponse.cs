namespace OllamaClient.Json
{
    public record struct ListModelsResponse
    {
        public ModelInfo[] models { get; set; }
    }
}