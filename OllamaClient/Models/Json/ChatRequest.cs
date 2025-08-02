namespace OllamaClient.Models.Json
{
    public record struct ChatRequest
    {
        public string model { get; set; }
        public Message[] messages { get; set; }
        public string? role { get; set; }
        public bool? stream { get; set; }
        public ModelParameters? model_parameters { get; set; }
        public string? keep_alive { get; set; }
    }
}
