namespace OllamaClient.Services.Json
{
    public record struct ListModelsResponse
    {
        public ModelInfo[] models { get; set; }
    }
}