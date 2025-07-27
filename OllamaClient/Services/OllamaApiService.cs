using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaClient.Models;
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

        private async Task<DelimitedJsonStream<T>> GetJsonStream<T>(HttpRequestMessage request, JsonTypeInfo<T> jsonTypeInfo)
        {
            HttpResponseMessage httpResp = await _HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (httpResp.IsSuccessStatusCode)
            {
                return new(await httpResp.Content.ReadAsStreamAsync(), '\n', jsonTypeInfo);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }

        }

        public async Task<DelimitedJsonStream<ChatResponse>> ChatStream(ChatRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Chat)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ChatRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.ChatResponse);
        }

        public async Task<DelimitedJsonStream<CompletionResponse>> CompletionStream(CompletionRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.GenerateCompletion)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CompletionRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.CompletionResponse);
        }

        public async Task<DelimitedJsonStream<StatusResponse>> CreateModelStream(CreateModelRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Create)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CreateModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public async Task<ListModelsResponse> ListModels()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, _Endpoints.List);
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ListModelsResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<ShowModelResponse> ShowModel(ShowModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Show)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ShowModelRequest)
            };
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ShowModelResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<bool> CopyModel(CopyModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Copy)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CopyModelRequest)
            };
            using HttpResponseMessage resp = await _HttpClient.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteModel(DeleteModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Delete, _Endpoints.Delete)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.DeleteModelRequest)
            };
            using HttpResponseMessage resp = await _HttpClient.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<DelimitedJsonStream<StatusResponse>> PullModelStream(PullModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, _Endpoints.Pull)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.PullModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public async Task<RunningModelsResponse> ListRunningModels()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, _Endpoints.Ps);
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.RunningModelsResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<VersionResponse> GetVersion()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, _Endpoints.Version);
            using HttpResponseMessage httpResp = await _HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.VersionResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }
    }
}
