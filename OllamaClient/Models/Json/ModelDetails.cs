namespace OllamaClient.Models.Json
{
    public record struct ModelDetails
    {
        public string? parent_model { get; set; }
        public string format { get; set; }
        public string family { get; set; }
        public string[]? families { get; set; }
        public string parameter_size { get; set; }
        public string quantization_level { get; set; }
    }
}
