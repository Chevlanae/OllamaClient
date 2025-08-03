using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaClient.Json;
using OllamaClient.Models;
using OllamaClient.Models.Json;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    internal class OllamaApiService
    {
        public class Settings
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public string SocketAddress { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public bool UseHttps { get; set; }
            public TimeSpan RequestTimeout { get; set; }
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

        private ILogger _Logger { get; set; }
        private Settings _Settings { get; set; }
        private Endpoints _Endpoints { get; set; }
        private HttpClient _HttpClient { get; set; }

        public OllamaApiService(ILogger<OllamaApiService> logger, IOptions<Settings> settings)
        {
            _Logger = logger;
            _Settings = settings.Value;
            _Endpoints = new(_Settings.SocketAddress, _Settings.UseHttps);
            _HttpClient = new HttpClient
            {
                Timeout = _Settings.RequestTimeout
            };
        }

        public async Task<DelimitedJsonStream<ChatResponse>> ChatStream(ChatRequest request)
        {
            request.stream = true;

            _Logger.LogDebug("POST {Url}", _Endpoints.Chat);

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Chat)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ChatRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.ChatResponse);
        }

        public async Task<DelimitedJsonStream<CompletionResponse>> CompletionStream(CompletionRequest request)
        {
            request.stream = true;

            _Logger.LogDebug("POST {Url}", _Endpoints.GenerateCompletion);

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.GenerateCompletion)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CompletionRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.CompletionResponse);
        }

        public async Task<DelimitedJsonStream<StatusResponse>> CreateModelStream(CreateModelRequest request)
        {
            request.stream = true;

            _Logger.LogDebug("POST {Url}", _Endpoints.Create);

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Create)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CreateModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public async Task<ListModelsResponse> ListModels()
        {
            _Logger.LogDebug("GET {Url}", _Endpoints.List);
            using HttpRequestMessage req = new(HttpMethod.Get, _Endpoints.List);
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode} - Returned JSON object", req.RequestUri?.OriginalString, httpResp.StatusCode);
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ListModelsResponse);
            }
            else
            {
                Exception err = new HttpRequestException($"{httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, httpResp.StatusCode);
                throw err;
            }
        }

        public async Task<ShowModelResponse> ShowModel(ShowModelRequest request)
        {
            _Logger.LogDebug("POST {Url}", _Endpoints.Show);
            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Show)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ShowModelRequest)
            };
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode} - Returned JSON object", req.RequestUri?.OriginalString, httpResp.StatusCode);
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ShowModelResponse);
            }
            else
            {
                Exception err = new HttpRequestException($"{httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, httpResp.StatusCode);
                throw err;
            }
        }

        public async Task<bool> CopyModel(CopyModelRequest request)
        {
            _Logger.LogDebug("POST {Url}", _Endpoints.Copy);
            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Copy)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CopyModelRequest)
            };
            using HttpResponseMessage resp = await _HttpClient.SendAsync(req);

            if (resp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, resp.StatusCode);
            }
            else
            {
                Exception err = new HttpRequestException($"{resp.StatusCode} - {await resp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, resp.StatusCode);
            }

            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteModel(DeleteModelRequest request)
        {
            _Logger.LogDebug("DELETE {Url}", _Endpoints.Delete);
            using HttpRequestMessage req = new(HttpMethod.Delete, _Endpoints.Delete)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.DeleteModelRequest)
            };
            using HttpResponseMessage resp = await _HttpClient.SendAsync(req);

            if (resp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, resp.StatusCode);
            }
            else
            {
                Exception err = new HttpRequestException($"{resp.StatusCode} - {await resp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, resp.StatusCode);
            }
            return resp.IsSuccessStatusCode;
        }

        public async Task<DelimitedJsonStream<StatusResponse>> PullModelStream(PullModelRequest request)
        {
            request.stream = true;

            _Logger.LogDebug("POST {Url}", _Endpoints.Pull);

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Pull)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.PullModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public async Task<RunningModelsResponse> ListRunningModels()
        {
            _Logger.LogDebug("GET {Url}", _Endpoints.Ps);
            using HttpRequestMessage req = new(HttpMethod.Get, _Endpoints.Ps);
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode} - Returned JSON object", req.RequestUri?.OriginalString, httpResp.StatusCode);
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.RunningModelsResponse);
            }
            else
            {
                Exception err = new HttpRequestException($"{httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, httpResp.StatusCode);
                throw err;
            }
        }

        public async Task<VersionResponse> GetVersion()
        {
            _Logger.LogDebug("GET {Url}", _Endpoints.Version);
            using HttpRequestMessage req = new(HttpMethod.Get, _Endpoints.Version);
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode} - Returned JSON object", req.RequestUri?.OriginalString, httpResp.StatusCode);
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.VersionResponse);
            }
            else
            {
                Exception err = new HttpRequestException($"{httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", req.RequestUri?.OriginalString, httpResp.StatusCode);
                throw err;
            }
        }

        private async Task<DelimitedJsonStream<T>> GetJsonStream<T>(HttpRequestMessage request, JsonTypeInfo<T> jsonTypeInfo)
        {
            HttpResponseMessage httpResp = await _HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (httpResp.IsSuccessStatusCode)
            {
                _Logger.LogDebug("RESPONSE {Url} - {StatusCode} - Returned JSON stream", request.RequestUri?.OriginalString, httpResp.StatusCode);
                return new(await httpResp.Content.ReadAsStreamAsync(), '\n', jsonTypeInfo);
            }
            else
            {
                Exception err = new HttpRequestException($"{httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
                _Logger.LogDebug(err, "RESPONSE {Url} - {StatusCode}", request.RequestUri?.OriginalString, httpResp.StatusCode);
                throw err;
            }

        }
    }
}
