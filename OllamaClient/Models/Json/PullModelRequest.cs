namespace OllamaClient.Models.Json
{
    public record struct PullModelRequest
    {
        public string model { get; set; }
        public bool insecure { get; set; }
        public bool stream { get; set; }
    }
}