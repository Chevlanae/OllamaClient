namespace OllamaClient.Models.Json
{
    public record struct CopyModelRequest
    {
        public string source { get; set; }
        public string destination { get; set; }
    }
}