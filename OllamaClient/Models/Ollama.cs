using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.Models.Ollama
{
    internal static class ProcessTracker
    {
        private static Process[] Detections { get; set; }

        static ProcessTracker()
        {
            Detections = [];
        }

        public static bool Scan()
        {
            Detections = Process.GetProcessesByName("ollama");

            return Detections.Length > 0 && Detections.All((p) => { return !p.HasExited; });
        }
    }

    public static class Patterns
    {
        public static string FROM = "FROM\\s.*";
        public static string TEMPLATE = "TEMPLATE\\s\"\"\"\"\"\"(.|\\s)*?\"\"\"\"\"\"";
        public static string PARAMETER = "PARAMETER\\s.*\\s.*";
        public static string SYSTEM = "SYSTEM\\s\"\"\"\"\"\"(.|\\s)*?\"\"\"\"\"\"";
        public static string ADAPTER = "ADAPTER\\s.*";
        public static string MESSAGE = "MESSAGE\\s(user|assistant|system|tool)\\s.*";
    }

    public static class RegularExpressions
    {
        public static Regex FROM = new(Patterns.FROM, RegexOptions.IgnoreCase);
        public static Regex TEMPLATE = new(Patterns.TEMPLATE, RegexOptions.IgnoreCase);
        public static Regex PARAMETER = new(Patterns.PARAMETER, RegexOptions.IgnoreCase);
        public static Regex SYSTEM = new(Patterns.SYSTEM, RegexOptions.IgnoreCase);
        public static Regex ADAPTER = new(Patterns.ADAPTER, RegexOptions.IgnoreCase);
        public static Regex MESSAGE = new(Patterns.MESSAGE, RegexOptions.IgnoreCase);
    }

    public enum Role
    {
        user,
        system,
        assistant,
        tool
    }

    public record struct Message
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
        public int? top_k { get; set; }
        public float? top_p { get; set; }
        public float? min_p { get; set; }
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

    public record struct CompletionRequest
    {
        public string model { get; set; }
        public string prompt { get; set; }
        public bool? stream { get; set; }
        public string? system { get; set; }
        public string? template { get; set; }
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
        public object? template { get; set; }
        public string[]? license { get; set; }
        public string? system { get; set; }
        public ModelParameters? parameters { get; set; }
        public Message[]? messages { get; set; }
        public bool? stream { get; set; }
        public string? quantize { get; set; }
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
        public string modified_at { get; set; }
        public long size { get; set; }
        public string digest { get; set; }
        public ModelDetails details { get; set; }
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

    public record struct ShowModelRequest
    {
        public string model { get; set; }
    }

    public record struct ShowModelResponse
    {
        public string modelfile { get; set; }
        public string parameters { get; set; }
        public string template { get; set; }
        public ModelDetails details { get; set; }
        public object model_info { get; set; }
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

    /// <summary>
    /// A stream reader that reads a stream of JSON objects delimited by a given character
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DelimitedJsonStream<T> : IDisposable
    {
        private StreamReader Reader { get; set; }
        private StringBuilder PartialObject { get; set; }
        private JsonTypeInfo<T> JsonTypeInfo { get; set; }

        public Stream BaseStream { get; set; }
        public char Delimiter { get; set; }
        public bool EndOfStream => Reader.EndOfStream;

        /// <summary>
        /// Create a new DelimitedJsonStream with given BaseStream and Delimiter
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <param name="delimiter"></param>
        public DelimitedJsonStream(Stream sourceStream, char delimiter, JsonTypeInfo<T> jsonTypeInfo)
        {
            PartialObject = new();
            JsonTypeInfo = jsonTypeInfo;
            BaseStream = sourceStream;
            Delimiter = delimiter;
            Reader = new(BaseStream);
        }

        public void Dispose()
        {
            BaseStream.Dispose();
            Reader.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Read the stream and deserialize JSON objects into given IProgress parameter
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public async Task Read(IProgress<T> progress, CancellationToken cancellationToken, int bufferSize = 32)
        {
            //buffer
            char[] buffer = new char[bufferSize];
            while (!Reader.EndOfStream)
            {
                //read BaseStream
                await Reader.ReadAsync(buffer, cancellationToken);
                foreach (char c in buffer)
                {
                    //check for delimiter, else append char to PartialObject
                    if (c == Delimiter)
                    {
                        try
                        {
                            //Cleanup any leading characters preceding the opening bracket
                            string objString = Regex.Replace(PartialObject.ToString(), "(^.*?{){1}", "{");

                            //Serialize PartialObject and pass serialized object to given IProgess parameter
                            T? obj = JsonSerializer.Deserialize(objString, JsonTypeInfo);
                            if (obj != null)
                            {
                                progress.Report(obj);
                            }
                        }
                        //skip object if deserialize operation fails
                        catch (JsonException)
                        {
                            continue;
                        }
                        finally
                        {
                            PartialObject.Clear();
                        }
                    }
                    else
                    {
                        PartialObject.Append(c);
                    }
                }
            }
        }
    }

    public class Endpoints
    {
        public string BaseUrl { get; private set; }
        public string GenerateCompletion { get; private set; }
        public string Chat { get; private set; }
        public string Create { get; private set; }
        public string List { get; private set; }
        public string Show { get; private set; }
        public string Copy { get; private set; }
        public string Delete { get; private set; }
        public string Pull { get; private set; }
        public string Push { get; private set; }
        public string Embed { get; private set; }
        public string Ps { get; private set; }
        public string Version { get; private set; }

        public Endpoints(string socketAddress, bool useHttps = false)
        {
            BaseUrl = useHttps ? "https" : "http" + "://" + socketAddress + "/api/";
            GenerateCompletion = BaseUrl + "generate";
            Chat = BaseUrl + "chat";
            Create = BaseUrl + "create";
            List = BaseUrl + "tags";
            Show = BaseUrl + "show";
            Copy = BaseUrl + "copy";
            Delete = BaseUrl + "delete";
            Pull = BaseUrl + "pull";
            Push = BaseUrl + "push";
            Embed = BaseUrl + "embed";
            Ps = BaseUrl + "ps";
            Version = BaseUrl + "version";
        }
    }

    public class ClientOptions(string socketAddress, bool useHttps, TimeSpan requestTimeout)
    {
        public string SocketAddress { get; set; } = socketAddress;
        public bool UseHttps { get; set; } = useHttps;
        public TimeSpan RequestTimeout { get; set; } = requestTimeout;
    }

    public static class ApiClient
    {
        private static Endpoints Endpoints { get; set; }

        private static HttpClient HttpClient { get; set; }

        static ApiClient()
        {
            Endpoints = new Endpoints(Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "localhost:11434");
            HttpClient = new HttpClient();
            HttpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        public static void SetOptions(ClientOptions options)
        {
            Endpoints = new Endpoints(options.SocketAddress, options.UseHttps);
            HttpClient.Timeout = options.RequestTimeout;
        }

        private static async Task<DelimitedJsonStream<T>?> GetJsonStream<T>(HttpRequestMessage request, JsonTypeInfo<T> jsonTypeInfo)
        {
            HttpResponseMessage httpResp = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (httpResp.IsSuccessStatusCode)
            {
                return new(await httpResp.Content.ReadAsStreamAsync(), '\n', jsonTypeInfo);
            }
            return null;
        }

        public static async Task<DelimitedJsonStream<ChatResponse>?> ChatStream(ChatRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Chat)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ChatRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.ChatResponse);
        }

        public static async Task<DelimitedJsonStream<CompletionResponse>?> CompletionStream(CompletionRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.GenerateCompletion)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CompletionRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.CompletionResponse);
        }

        public static async Task<DelimitedJsonStream<StatusResponse>?> CreateModel(CreateModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Create)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CreateModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public static async Task<ListModelsResponse?> ListModels()
        {
            using (HttpRequestMessage req = new(HttpMethod.Get, Endpoints.List))
            using (HttpResponseMessage resp = await HttpClient.SendAsync(req))
            {
                if (resp.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize(await resp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ListModelsResponse);
                }
            }

            return null;
        }

        public static async Task<ShowModelResponse?> ShowModel(ShowModelRequest request)
        {
            using (HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Show)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ShowModelRequest)
            })
            using (HttpResponseMessage resp = await HttpClient.SendAsync(req))
            {
                if (resp.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize(await resp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ShowModelResponse);
                }
            }
            return null;
        }

        public static async Task<bool> CopyModel(CopyModelRequest request)
        {
            using (HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Copy)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CopyModelRequest)
            })
            using (HttpResponseMessage resp = await HttpClient.SendAsync(req))
            {
                return resp.IsSuccessStatusCode;
            }
        }

        public static async Task<bool> DeleteModel(DeleteModelRequest request)
        {
            using (HttpRequestMessage req = new(HttpMethod.Delete, Endpoints.Delete)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.DeleteModelRequest)
            })
            using (HttpResponseMessage resp = await HttpClient.SendAsync(req))
            {
                return resp.IsSuccessStatusCode;
            }
        }

        public static async Task<DelimitedJsonStream<StatusResponse>?> PullModelStream(PullModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Pull)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.PullModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public static async Task<RunningModelsResponse?> ListRunningModels()
        {
            using (HttpRequestMessage req = new(HttpMethod.Get, Endpoints.Ps))
            using (HttpResponseMessage resp = await HttpClient.SendAsync(req))
            {
                if (resp.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize(await resp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.RunningModelsResponse);
                }
            }
            return null;
        }

        public static async Task<VersionResponse?> GetVersion()
        {
            using (HttpRequestMessage req = new(HttpMethod.Get, Endpoints.Version))
            using (HttpResponseMessage resp = await HttpClient.SendAsync(req))
            {
                if (resp.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize(await resp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.VersionResponse);
                }
            }
            return null;
        }
    }
}