using System.Collections.Generic;

namespace OllamaClient.Services.Json
{
    public record struct FunctionParameters
    {
        public string type { get; set; }
        public Dictionary<string, FunctionParameterProperty> properties { get; set; }
        public string[]? required { get; set; }

        public FunctionParameters()
        {
            type = "object";
            properties = new();
            required = [];
        }
    }
}
