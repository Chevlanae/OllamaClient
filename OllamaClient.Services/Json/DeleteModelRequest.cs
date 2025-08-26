namespace OllamaClient.Services.Json
{
    public record struct DeleteModelRequest
    {
        public string model { get; set; }
    }
}