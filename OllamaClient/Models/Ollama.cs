using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.Models.Ollama
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
        string Value { get; set;  }
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
        public string modified_at { get; set; }
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
        public string? template { get; set; }
        public string? license { get; set; }
        public string? system { get; set; }
        public ModelParameters? parameters { get; set; }
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
        public ModelDetails? details { get; set; }
        public object? model_info { get; set; }
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
        /// Reads the source stream into a string and attempts to deserialize the string at each delimiter into an object of type T.
        /// Passes any deserialized objects to the given IProgress parameter.
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
                            //Cleanup any leading characters preceding the opening bracket.
                            //This pattern: "(^.*?{){1}" will match any characters before the first
                            //opening bracket of the json string, including the first opening bracket, only once.
                            string objString = Regex.Replace(PartialObject.ToString(), "(^.*?{){1}", "{");

                            //Serialize PartialObject and pass serialized object to given IProgess parameter if obj not null
                            T? obj = JsonSerializer.Deserialize(objString, JsonTypeInfo);
                            if (obj is not null)
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

    public static class Api
    {
        public class ClientOptions(string socketAddress, bool useHttps, TimeSpan requestTimeout)
        {
            public string SocketAddress { get; set; } = socketAddress;
            public bool UseHttps { get; set; } = useHttps;
            public TimeSpan RequestTimeout { get; set; } = requestTimeout;
        }

        private static Endpoints Endpoints { get; set; }

        private static HttpClient HttpClient { get; set; }

        static Api()
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

        public static async Task<DelimitedJsonStream<ChatResponse>?> ChatStream(ChatRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Chat)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ChatRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.ChatResponse);
        }

        public static async Task<DelimitedJsonStream<CompletionResponse>?> CompletionStream(CompletionRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.GenerateCompletion)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CompletionRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.CompletionResponse);
        }

        public static async Task<DelimitedJsonStream<StatusResponse>?> CreateModelStream(CreateModelRequest request)
        {
            request.stream = true;

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
                else return null;
            }
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
                else return null;
            }
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
                else return null;
            }
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
                else return null;
            }
        }

        private static async Task<DelimitedJsonStream<T>?> GetJsonStream<T>(HttpRequestMessage request, JsonTypeInfo<T> jsonTypeInfo)
        {
            HttpResponseMessage httpResp = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (httpResp.IsSuccessStatusCode)
            {
                return new(await httpResp.Content.ReadAsStreamAsync(), '\n', jsonTypeInfo);
            }
            else return null;
        }
    }

    public class ModelFile
    {
        public static class RegularExpressions
        {
            public static class Patterns
            {
                private static string TripleQuoteString = "\"\"\"\"\"\"(\\s|\\S)*?\"\"\"\"\"\"";
                private static string DoubleQuoteString = "\"(\\s|\\S)*?\"";
                private static string DoubleOrTripleQuoteString = $"({TripleQuoteString}|{DoubleQuoteString})";
                public static string FROM = "^FROM\\s.*";
                public static string TEMPLATE = $"^TEMPLATE\\s{DoubleOrTripleQuoteString}";
                public static string PARAMETER = "^PARAMETER\\s.*\\s.*?";
                public static string SYSTEM = $"^SYSTEM\\s.*";
                public static string ADAPTER = "^ADAPTER\\s.*";
                public static string LICENSE = $"^LICENSE\\s{DoubleOrTripleQuoteString}";
                public static string MESSAGE = "^MESSAGE\\s(user|assistant|system|tool)\\s.*";
            }

            public static Regex FROM = new(Patterns.FROM, RegexOptions.Compiled);
            public static Regex TEMPLATE = new(Patterns.TEMPLATE, RegexOptions.Compiled);
            public static Regex PARAMETER = new(Patterns.PARAMETER, RegexOptions.Compiled);
            public static Regex SYSTEM = new(Patterns.SYSTEM, RegexOptions.Compiled);
            public static Regex ADAPTER = new(Patterns.ADAPTER, RegexOptions.Compiled);
            public static Regex LICENSE = new(Patterns.LICENSE, RegexOptions.Compiled);
            public static Regex MESSAGE = new(Patterns.MESSAGE, RegexOptions.Compiled);
        }

        private enum Instruction
        {
            FROM,
            TEMPLATE,
            PARAMETER,
            SYSTEM,
            ADAPTER,
            LICENSE,
            MESSAGE
        }

        public string From { get; set; }
        public string? Template { get; set; }
        public ModelParameters? Parameters { get; set; }
        public string? System { get; set; }
        public string? Adapter { get; set; }
        public string? License { get; set; }
        public List<Message>? Messages { get; set; }

        /// <summary>
        ///  Parses the given file string into a ModelFile object.
        /// </summary>
        /// <param name="fileString"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="OverflowException" />
        public ModelFile(string fileString)
        {
            Match fromMatch = RegularExpressions.FROM.Match(fileString);
            Match templateMatch = RegularExpressions.TEMPLATE.Match(fileString);
            MatchCollection parameterMatches = RegularExpressions.PARAMETER.Matches(fileString);
            Match systemMatch = RegularExpressions.SYSTEM.Match(fileString);
            Match adapterMatch = RegularExpressions.ADAPTER.Match(fileString);
            Match licenseMatch = RegularExpressions.LICENSE.Match(fileString);
            MatchCollection messageMatches = RegularExpressions.MESSAGE.Matches(fileString);

            if (fromMatch.Success) From = ParseInstructionValue(Instruction.FROM, fromMatch.Value);
            else throw new ArgumentException($"Invalid file string. {RegularExpressions.Patterns.FROM} pattern is missing from the source.");

            if (templateMatch.Success) Template = ParseInstructionValue(Instruction.TEMPLATE, templateMatch.Value);

            if(parameterMatches.Count > 0) Parameters = new();

            foreach (Match parameter in parameterMatches)
            {
                if (parameter.Success)
                {
                    ParseAndAggregateModelParameter(parameter.Value);
                }
            }

            if (systemMatch.Success) System = ParseInstructionValue(Instruction.SYSTEM, systemMatch.Value);

            if (adapterMatch.Success) Adapter = ParseInstructionValue(Instruction.ADAPTER, adapterMatch.Value);

            if (licenseMatch.Success) License = ParseInstructionValue(Instruction.LICENSE, licenseMatch.Value);

            if(messageMatches.Count > 0) Messages = [];

            foreach (Match message in messageMatches)
            {
                if (message.Success)
                {
                    ParseAndAggregateMessage(message.Value);
                }
            }
        }

        /// <summary>
        /// Parses the given source string into a ModelParameter object and aggregates it into Parameters.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="OverflowException" />
        private void ParseAndAggregateModelParameter(string sourceString)
        {
            string parameterValueString = ParseInstructionValue(Instruction.PARAMETER, sourceString);

            string[] keyValue = parameterValueString.Split(" ", 2, StringSplitOptions.TrimEntries);

            if(Parameters is not null)
            {
                AggregateModelParameter(Enum.Parse<ModelParameterKey>(keyValue[0]), keyValue[1], Parameters.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceString"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        private void ParseAndAggregateMessage(string sourceString)
        {
            string parameterValueString = ParseInstructionValue(Instruction.MESSAGE, sourceString);

            string[] keyValue = parameterValueString.Split(" ", 2, StringSplitOptions.TrimEntries);

            Messages?.Add(new (Enum.Parse<Role>(keyValue[0]), keyValue[1]));
        }

        /// <summary>
        /// Parses the given string for a model file parameter ("FROM input", "TEMPLATE \"\"\"input\"\"\"", etc.), and returns the value (input).
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        private string ParseInstructionValue(Instruction instruction, string sourceString)
        {
            string value = string.Empty;

            switch (instruction)
            {
                case Instruction.FROM:
                    value = sourceString.Replace("FROM ", string.Empty);
                    break;
                case Instruction.TEMPLATE:
                    value = sourceString.Replace("TEMPLATE ", string.Empty);
                    break;
                case Instruction.PARAMETER:
                    value = sourceString.Replace("PARAMETER ", string.Empty);
                    break;
                case Instruction.SYSTEM:
                    value = sourceString.Replace("SYSTEM ", string.Empty);
                    break;
                case Instruction.ADAPTER:
                    value = sourceString.Replace("ADAPTER ", string.Empty);
                    break;
                case Instruction.LICENSE:
                    value = sourceString.Replace("LICENSE ", string.Empty);
                    break;
                case Instruction.MESSAGE:
                    value = sourceString.Replace("MESSAGE ", string.Empty);
                    break;
            }

            return value;
        }

        /// <summary>
        /// Aggregates the given IModelParameter into the given ModelParameters object.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="FormatException" />
        /// <exception cref="OverflowException" />"
        public static void AggregateParameter(IModelParameter item, ModelParameters target)
        {
            AggregateModelParameter(item.Key, item.Value, target);
        }


        /// <summary>
        /// Aggregates the given IModelParameter into the given ModelParameters object.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="FormatException" />
        /// <exception cref="OverflowException" />"
        public static void AggregateModelParameter(ModelParameterKey key, string value, ModelParameters target)
        {
            switch (key)
            {
                case ModelParameterKey.mirostat:
                    target.mirostat = int.Parse(value);
                    break;
                case ModelParameterKey.mirostat_eta:
                    target.mirostat_eta = float.Parse(value);
                    break;
                case ModelParameterKey.mirostat_tau:
                    target.mirostat_tau = float.Parse(value);
                    break;
                case ModelParameterKey.num_ctx:
                    target.num_ctx = int.Parse(value);
                    break;
                case ModelParameterKey.repeat_last_n:
                    target.repeat_last_n = int.Parse(value);
                    break;
                case ModelParameterKey.repeat_penalty:
                    target.repeat_penalty = float.Parse(value);
                    break;
                case ModelParameterKey.temperature:
                    target.temperature = float.Parse(value);
                    break;
                case ModelParameterKey.seed:
                    target.seed = int.Parse(value);
                    break;
                case ModelParameterKey.stop:
                    target.stop = value;
                    break;
                case ModelParameterKey.num_predict:
                    target.num_predict = int.Parse(value);
                    break;
                case ModelParameterKey.top_k:
                    target.top_k = int.Parse(value);
                    break;
                case ModelParameterKey.top_p:
                    target.top_p = float.Parse(value);
                    break;
                case ModelParameterKey.min_p:
                    target.min_p = float.Parse(value);
                    break;
            }
        }

        public new string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"FROM {From}");
            if (Template is not null) sb.AppendLine($"TEMPLATE {Template}");
            
            if (Parameters is not null)
            {
                foreach (PropertyInfo info in Parameters.GetType().GetProperties())
                {
                    object? obj = info.GetValue(Parameters);

                    if (obj is not null)
                    {
                        sb.AppendLine($"PARAMETER {info.Name} {obj.ToString()}");
                    }
                }
            }

            if (System is not null) sb.AppendLine($"SYSTEM {System}");
            if (Adapter is not null) sb.AppendLine($"ADAPTER {Adapter}");
            if (License is not null) sb.AppendLine($"LICENSE {License}");
            if (Messages is not null)
            {
                foreach (Message message in Messages)
                {
                    sb.AppendLine($"MESSAGE {message.role} {message.content}");
                }
            }
            return sb.ToString();
        }
    }
}