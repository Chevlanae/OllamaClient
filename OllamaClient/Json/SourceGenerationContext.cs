using OllamaClient.Json;
using System.Text.Json.Serialization;

namespace OllamaClient.Models.Json
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [JsonSerializable(typeof(ChatRequest))]
    [JsonSerializable(typeof(ChatResponse))]
    [JsonSerializable(typeof(CompletionRequest))]
    [JsonSerializable(typeof(CompletionResponse))]
    [JsonSerializable(typeof(ListModelsResponse))]
    [JsonSerializable(typeof(StatusResponse))]
    [JsonSerializable(typeof(CreateModelRequest))]
    [JsonSerializable(typeof(ShowModelRequest))]
    [JsonSerializable(typeof(ShowModelResponse))]
    [JsonSerializable(typeof(CopyModelRequest))]
    [JsonSerializable(typeof(DeleteModelRequest))]
    [JsonSerializable(typeof(PullModelRequest))]
    [JsonSerializable(typeof(RunningModelInfo))]
    [JsonSerializable(typeof(RunningModelsResponse))]
    [JsonSerializable(typeof(VersionResponse))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}