namespace OllamaClient.Json
{
    public record struct CompletionRequest
    {
        public string model { get; set; }
        public string prompt { get; set; }
        public bool? stream { get; set; }
        public string? system { get; set; }
        public string? template { get; set; }
        public ModelParameters? options { get; set; }
    }
}
