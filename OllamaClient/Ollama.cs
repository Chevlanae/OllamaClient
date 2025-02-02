using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            string role { get; set; }
            string content { get; set; }

            public Message(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<Message>(data, options);
            }
        }

        public struct ModelParameters
        {
            int? mirostat { get; set; }
            float? mirostat_eta { get; set; }
            float? mirostat_tau { get; set; }
            int? num_ctx { get; set; }
            int? repeat_last_n { get; set; }
            float? repeat_penalty { get; set; }
            float? temperature { get; set; }
            int? seed { get; set; }
            string? stop { get; set; }
            int? num_predict { get; set; }
            int? top_k { get; set; }
            float? top_p { get; set; }
            float? min_p { get; set; }

            public ModelParameters(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<ModelParameters>(data, options);
            }
        }

        public struct ChatRequest
        {
            string model { get; set; }
            Message[] messages { get; set; }
            string? role { get; set; }
            bool? streaam { get; set; }
            ModelParameters? model_parameters { get; set; }
            string? keep_alive { get; set; }

            public ChatRequest(object data)
            {
                this = (ChatRequest)data;
            }
        }

        public struct ChatResponse
        {
            string? model { get; set; }
            string? created_at { get; set; }
            Message? message { get; set; }
            bool? done { get; set; }
            string? done_reason { get; set; }
            long? total_duration { get; set; }
            long? load_duration { get; set; }
            int? prompt_eval_count { get; set; }
            long? prompt_eval_duration { get; set; }
            int? eval_count { get; set; }
            long? eval_duration { get; set; }

            public ChatResponse(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<ChatResponse>(data, options);
            }
        }

        public struct CompletionRequest
        {
            string model { get; set; }
            string prompt { get; set; }
            bool? stream { get; set; }
            string? system { get; set; }
            string? template { get; set; }
            ModelParameters? options { get; set; }

            public CompletionRequest(object data)
            {
                this = (CompletionRequest)data;
            }
        }

        public struct CompletionResponse
        {
            string? model { get; set; }
            string? created_at { get; set; }
            bool? done { get; set; }
            long? total_duration { get; set; }
            long? load_duration { get; set; }
            int? prompt_eval_count { get; set; }
            long? prompt_eval_duration { get; set; }
            int? eval_count { get; set; }
            long? eval_duration { get; set; }
            int[]? context { get; set; }
            string? response { get; set; }


            public CompletionResponse(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<CompletionResponse>(data, options);
            }
        }

        public struct CreateModelRequest
        {
            string model { get; set; }
            string? from { get; set; }
            Dictionary<string, string>? files { get; set; }
            Dictionary<string, string>? adapters { get; set; }
            object? template { get; set; }
            string[]? license { get; set; }
            string? system { get; set; }
            ModelParameters? parameters { get; set; }
            Message[]? messages { get; set; }
            bool? stream { get; set; }
            string? quantize { get; set; }

            public CreateModelRequest(Stream data, JsonSerializerOptions options)
            {
                this = JsonSerializer.Deserialize<CreateModelRequest>(data, options);
            }
        }

        internal class Endpoints
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

        internal class Connection
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

            public async Task Chat(ChatRequest request, Action<ChatResponse> callback)
            {
                HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Chat)
                {
                    Content = JsonContent.Create(request)
                };
                await GetJsonStream(req, callback);
            }

            public async Task GenerateCompletion(CompletionRequest request, Action<CompletionResponse> callback)
            {
                HttpRequestMessage req = new(HttpMethod.Post, Endpoints.GenerateCompletion)
                {
                    Content = JsonContent.Create(request)
                };
                await GetJsonStream(req, callback);
            }

            private async Task GetJsonStream<T>(HttpRequestMessage request, Action<T> callback)
            {
                try
                {
                    HttpResponseMessage httpResp = await _HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    using DelimitedJsonStream<T> stream = new(await httpResp.Content.ReadAsStreamAsync(), '\n');
                    stream.ObjectReceived += (sender, obj) =>
                    {
                        callback(obj);
                    };
                    await stream.Read();
                }
                catch (HttpRequestException e)
                {
                    Debug.Write(e);
                }
            }
        }

        public class DelimitedJsonStream<T> : IDisposable
        {
            public event EventHandler<T>? ObjectReceived;

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
                Objects = new();
                _Reader = new(BaseStream);
                _PartialObject = "";
            }

            public void Dispose()
            {
                BaseStream.Dispose();
                _Reader.Dispose();
            }

            protected void OnObjectReceived(T obj)
            {
                ObjectReceived?.Invoke(this, obj);
            }

            public async Task Read()
            {
                char[] buffer = new char[256];
                while (!_Reader.EndOfStream)
                {

                    await _Reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    foreach (char c in buffer)
                    {
                        if (c == Delimiter && _PartialObject[-1] == '}')
                        {
                            _PartialObject = Regex.Replace(_PartialObject, "^.*{", "{");
                            try
                            {
                                if (JsonSerializer.Deserialize<T>(_PartialObject) is T obj)
                                {
                                    Objects.Add(obj);
                                    OnObjectReceived(obj);
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