using System;

namespace OllamaClient.Json
{
    public record struct ShowModelResponse
    {
        public string? license { get; set; }
        public string modelfile { get; set; }
        public string? parameters { get; set; }
        public string? template { get; set; }
        public string? system { get; set; }
        public ModelDetails? details { get; set; }
        public object? model_info { get; set; }
        public TensorInfo[]? tensors { get; set; }
        public string[]? capabilities { get; set; }
        public DateTime? modified_at { get; set; }
    }
}