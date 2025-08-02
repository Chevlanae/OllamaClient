using System;

namespace OllamaClient.Models.Json
{
    public record struct RunningModelInfo
    {
        public string name { get; set; }
        public string model { get; set; }
        public long size { get; set; }
        public string digest { get; set; }
        public ModelDetails details { get; set; }
        public DateTime expires_at { get; set; }
        public long size_vram { get; set; }
    }
}
