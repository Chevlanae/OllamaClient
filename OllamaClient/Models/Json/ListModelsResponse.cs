namespace OllamaClient.Models.Json
{
    public record struct ListModelsResponse
    {
        public ModelInfo[] models { get; set; }
    }
}