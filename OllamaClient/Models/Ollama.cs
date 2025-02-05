using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace OllamaClient
{
    namespace Api
    {
        public enum Role
        {
            user,
            system,
            assistant,
            tool
        }

        public struct Message
        {
            public string? role { get; set; }
            public string? content { get; set; }

            public Message(Role role, string content)
            {
                this.role = Enum.GetName(role);
                this.content = content;
            }

            public Message(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<Message>(data, options);
            }
        }

        public struct ModelParameters
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

            public ModelParameters(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<ModelParameters>(data, options);
            }
        }

        public struct ChatRequest
        {
            public string model { get; set; }
            public Message[] messages { get; set; }
            public string? role { get; set; }
            public bool? stream { get; set; }
            public ModelParameters? model_parameters { get; set; }
            public string? keep_alive { get; set; }

            public ChatRequest(object data)
            {
                this = (ChatRequest)data;
            }
        }

        public struct ChatResponse
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

            public ChatResponse(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<ChatResponse>(data, options);
            }
        }

        public struct CompletionRequest
        {
            public string model { get; set; }
            public string prompt { get; set; }
            public bool? stream { get; set; }
            public string? system { get; set; }
            public string? template { get; set; }
            public ModelParameters? options { get; set; }

            public CompletionRequest(object data)
            {
                this = (CompletionRequest)data;
            }
        }

        public struct CompletionResponse
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


            public CompletionResponse(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<CompletionResponse>(data, options);
            }
        }

        public struct CreateModelRequest
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

            public CreateModelRequest(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<CreateModelRequest>(data, options);
            }
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

            public Endpoints(string socketAddress)
            {
                BaseUrl = "http://" + socketAddress + "/api/";
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

        public class Connection
        {
            public string SocketAddress { get; set; }

            public Endpoints Endpoints { get; set; }

            public TimeSpan Timeout => _HttpClient.Timeout;

            private HttpClient _HttpClient { get; set; }

            public Connection(string socketAddress, TimeSpan timeout)
            {
                SocketAddress = socketAddress;
                Endpoints = new Endpoints(SocketAddress);
                _HttpClient = new HttpClient();
                _HttpClient.Timeout = Timeout;
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

            private async Task<DelimitedJsonStream<T>?> GetJsonStream<T>(HttpRequestMessage request)
            {
                try
                {
                    HttpResponseMessage httpResp = await _HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    return new(await httpResp.Content.ReadAsStreamAsync(), '\n');
                }
                catch (HttpRequestException e)
                {
                    Debug.Write(e);
                }

                return null;
            }
        }

        public partial class DelimitedJsonStream<T> : IDisposable
        {
            public Stream BaseStream { get; set; }
            public char Delimiter { get; set; }
            public List<T> Objects { get; set; }
            private StreamReader _Reader { get; set; }
            private string _PartialObject { get; set; }

            public bool EndOfStream => _Reader.EndOfStream;

            public DelimitedJsonStream(Stream sourceStream, char delimiter)
            {
                BaseStream = sourceStream;
                Delimiter = delimiter;
                Objects = [];
                _Reader = new(BaseStream);
                _PartialObject = "";
            }

            public void Dispose()
            {
                BaseStream.Dispose();
                _Reader.Dispose();
                GC.SuppressFinalize(this);
            }

            public async Task Read(CancellationToken cancellationToken, IProgress<T> progress)
            {
                char[] buffer = new char[256];
                while (!_Reader.EndOfStream)
                {

                    await _Reader.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    foreach (char c in buffer)
                    {
                        if (c == Delimiter && _PartialObject.LastOrDefault() == '}')
                        {
                            _PartialObject = Regex.Replace(_PartialObject, "(^.*?{){1}", "{");
                            try
                            {
                                if (JsonSerializer.Deserialize<T>(_PartialObject) is T obj)
                                {
                                    progress.Report(obj);
                                }
                            }
                            catch (JsonException)
                            {
                                continue;
                            }
                            finally
                            {
                                _PartialObject = "";
                            }
                        }
                        else
                        {
                            _PartialObject += c;
                        }
                    }
                }
            }
        }
    }
}