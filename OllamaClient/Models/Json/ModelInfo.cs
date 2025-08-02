using System;

namespace OllamaClient.Models.Json
{
    public record struct ModelInfo
    {
        public string name { get; set; }
        public string model { get; set; }
        public DateTime modified_at { get; set; }
        public long size { get; set; }
        public string digest { get; set; }
        public ModelDetails details { get; set; }
    }
}
