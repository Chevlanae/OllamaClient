using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace OllamaClient.Models
{
    public enum Role
    {
        user,
        system,
        assistant,
        tool
    }

    public enum ModelParameterKey
    {
        mirostat,
        mirostat_eta,
        mirostat_tau,
        num_ctx,
        repeat_last_n,
        repeat_penalty,
        temperature,
        seed,
        stop,
        num_predict,
        num_keep,
        top_k,
        top_p,
        min_p
    }

    public enum QuantizationType
    {
        q2_K,
        q3_K_L,
        q3_K_M,
        q3_K_S,
        q4_0,
        q4_1,
        q4_K_M,
        q4_K_S,
        q5_0,
        q5_1,
        q5_K_M,
        q5_K_S,
        q6_K,
        q8_0,
    }

    public interface IMessage
    {
        string? role { get; set; }
        string? content { get; set; }
    }

    public interface IModelParameter
    {
        ModelParameterKey Key { get; set; }
        string Value { get; set; }
    }

    public record struct ModelParameterKeyValue : IModelParameter
    {
        public ModelParameterKey Key { get; set; }
        public string Value { get; set; }
        public ModelParameterKeyValue(ModelParameterKey key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public record struct Message : IMessage
    {
        public string? role { get; set; }
        public string? content { get; set; }

        public Message(Role role, string content)
        {
            this.role = Enum.GetName(role);
            this.content = content;
        }
    }

    public record struct ModelParameters
    {
        public int? mirostat { get; set; }
        public float? mirostat_eta { get; set; }
        public float? mirostat_tau { get; set; }
        public int? num_ctx { get; set; }
        public int? repeat_last_n { get; set; }
        public float? repeat_penalty { get; set; }
        public float? temperature { get; set; }
        public int? seed { get; set; }
        public string? stop { get; set; }
        public int? num_predict { get; set; }
        public int? num_keep { get; set; }
        public int? top_k { get; set; }
        public float? top_p { get; set; }
        public float? min_p { get; set; }
    }

    public record struct ModelDetails
    {
        public string? parent_model { get; set; }
        public string format { get; set; }
        public string family { get; set; }
        public string[]? families { get; set; }
        public string parameter_size { get; set; }
        public string quantization_level { get; set; }
    }

    public record struct ModelInfo
    {
        public string name { get; set; }
        public string model { get; set; }
        public DateTime modified_at { get; set; }
        public long size { get; set; }
        public string digest { get; set; }
        public ModelDetails details { get; set; }
    }

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

    public record struct ChatRequest
    {
        public string model { get; set; }
        public Message[] messages { get; set; }
        public string? role { get; set; }
        public bool? stream { get; set; }
        public ModelParameters? model_parameters { get; set; }
        public string? keep_alive { get; set; }
    }

    public record struct ChatResponse
    {
        public string? model { get; set; }
        public string? created_at { get; set; }
        public Message? message { get; set; }
        public bool? done { get; set; }
        public string? done_reason { get; set; }
        public long? total_duration { get; set; }
        public long? load_duration { get; set; }
        public int? prompt_eval_count { get; set; }
        public long? prompt_eval_duration { get; set; }
        public int? eval_count { get; set; }
        public long? eval_duration { get; set; }
    }

    [DataContract]
    public record struct CompletionRequest
    {
        [DataMember]
        public string model { get; set; }
        [DataMember]
        public string prompt { get; set; }
        [DataMember]
        public bool? stream { get; set; }
        [DataMember]
        public string? system { get; set; }
        [DataMember]
        public string? template { get; set; }
        [DataMember]
        public ModelParameters? options { get; set; }
    }

    public record struct CompletionResponse
    {
        public string? model { get; set; }
        public string? created_at { get; set; }
        public bool? done { get; set; }
        public long? total_duration { get; set; }
        public long? load_duration { get; set; }
        public int? prompt_eval_count { get; set; }
        public long? prompt_eval_duration { get; set; }
        public int? eval_count { get; set; }
        public long? eval_duration { get; set; }
        public int[]? context { get; set; }
        public string? response { get; set; }
    }

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

    public record struct ListModelsResponse
    {
        public ModelInfo[] models { get; set; }
    }

    public record struct StatusResponse
    {
        public string status { get; set; }
        public string? digestname { get; set; }
        public long? total { get; set; }
        public long? completed { get; set; }
    }

    public record struct TensorInfo
    {
        public string name { get; set; }
        public string type { get; set; }
        public int[] shape { get; set; }
    }

    public record struct ShowModelRequest
    {
        public string model { get; set; }
    }

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

    public record struct CopyModelRequest
    {
        public string source { get; set; }
        public string destination { get; set; }
    }

    public record struct DeleteModelRequest
    {
        public string model { get; set; }
    }

    public record struct PullModelRequest
    {
        public string model { get; set; }
        public bool insecure { get; set; }
        public bool stream { get; set; }
    }

    public record struct RunningModelsResponse
    {
        public RunningModelInfo[] models { get; set; }
    }

    public record struct VersionResponse
    {
        public string version { get; set; }
    }

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