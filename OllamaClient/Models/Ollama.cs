using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace OllamaClient.Models.Ollama
{
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
        public string? format { get; set; }
        public string? family { get; set; }
        public string[]? families { get; set; }
        public string? parameter_size { get; set; }
        public string? quantization_level { get; set; }
    }

    public record struct ModelInfo
    {
        public string? name { get; set; }
        public string? model { get; set; }
        public string? modified_at { get; set; }
        public long? size { get; set; }
        public string? digest { get; set; }
        public ModelDetails? details { get; set; }
    }

    public record struct ListModelsResponse
    {
        public ModelInfo[]? models { get; set; }
    }

    public class Endpoints
    {
        public string BaseUrl { get; set; }
        public string GenerateCompletion { get; set; }
        public string Chat { get; set; }
        public string Create { get; set; }
        public string List { get; set; }
        public string Show { get; set; }
        public string Copy { get; set; }
        public string Delete { get; set; }
        public string Pull { get; set; }
        public string Push { get; set; }
        public string Embed { get; set; }
        public string Ps { get; set; }
        public string Version { get; set; }

        public Endpoints()
        {
            BaseUrl = "http://" + (Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "localhost:11434") + "/api/";
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

    public class Client
    {
        public Endpoints Endpoints { get; set; }

        private HttpClient HttpClient { get; set; }

        public Client(TimeSpan? timeout)
        {
            Endpoints = new Endpoints();
            HttpClient = new HttpClient();
            HttpClient.Timeout = timeout ?? Timeout.InfiniteTimeSpan;
        }

        public async Task<DelimitedJsonStream<ChatResponse>?> ChatStream(ChatRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Chat)
            {
                Content = JsonContent.Create(request)
            };
            return await GetJsonStream<ChatResponse>(req);
        }

        public async Task<DelimitedJsonStream<CompletionResponse>?> GenerateCompletionStream(CompletionRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.GenerateCompletion)
            {
                Content = JsonContent.Create(request)
            };

            return await GetJsonStream<CompletionResponse>(req);
        }

        public async Task<ListModelsResponse?> ListModels()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, Endpoints.List);

            using HttpResponseMessage resp = await HttpClient.SendAsync(req);

            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ListModelsResponse>(await resp.Content.ReadAsStringAsync());
            }
            return null;
        }

        private async Task<DelimitedJsonStream<T>?> GetJsonStream<T>(HttpRequestMessage request)
        {
            HttpResponseMessage httpResp = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (httpResp.IsSuccessStatusCode)
            {
                return new(await httpResp.Content.ReadAsStreamAsync(), '\n');
            }
            return null;
        }
    }

    /// <summary>
    /// A stream reader that reads a stream of JSON objects delimited by a given character
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DelimitedJsonStream<T> : IDisposable
    {
        public Stream BaseStream { get; set; }
        public char Delimiter { get; set; }
        private StreamReader Reader { get; set; }
        private StringBuilder PartialObject { get; set; }

        public bool EndOfStream => Reader.EndOfStream;

        /// <summary>
        /// Create a new DelimitedJsonStream with given BaseStream and Delimiter
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <param name="delimiter"></param>
        public DelimitedJsonStream(Stream sourceStream, char delimiter)
        {
            BaseStream = sourceStream;
            Delimiter = delimiter;
            Reader = new(BaseStream);
            PartialObject = new();
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
                            T? obj = JsonSerializer.Deserialize<T>(objString);
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
}