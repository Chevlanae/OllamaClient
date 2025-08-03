using System.Collections.Generic;

namespace OllamaClient.Json
{
    public record struct CreateModelRequest
    {
        public string model { get; set; }
        public string? from { get; set; }
        public Dictionary<string, string>? files { get; set; }
        public Dictionary<string, string>? adapters { get; set; }
        public string? template { get; set; }
        public string? license { get; set; }
        public string? system { get; set; }
        public Dictionary<ModelParameterKey, object>? parameters { get; set; }
        public Message[]? messages { get; set; }
        public bool? stream { get; set; }
        public string? quantize { get; set; }
    }
}
