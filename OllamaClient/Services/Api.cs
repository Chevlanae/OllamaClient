using OllamaClient.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
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
            HttpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public static void SetOptions(ClientOptions options)
        {
            Endpoints = new Endpoints(options.SocketAddress, options.UseHttps);
            HttpClient.Timeout = options.RequestTimeout;
        }

        private static async Task<DelimitedJsonStream<T>> GetJsonStream<T>(HttpRequestMessage request, JsonTypeInfo<T> jsonTypeInfo)
        {
            HttpResponseMessage httpResp = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (httpResp.IsSuccessStatusCode)
            {
                return new(await httpResp.Content.ReadAsStreamAsync(), '\n', jsonTypeInfo);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }

        }

        public static async Task<DelimitedJsonStream<ChatResponse>> ChatStream(ChatRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Chat)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ChatRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.ChatResponse);
        }

        public static async Task<DelimitedJsonStream<CompletionResponse>> CompletionStream(CompletionRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.GenerateCompletion)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CompletionRequest)
            };

            return await GetJsonStream(req, SourceGenerationContext.Default.CompletionResponse);
        }

        public static async Task<DelimitedJsonStream<StatusResponse>> CreateModelStream(CreateModelRequest request)
        {
            request.stream = true;

            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Create)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CreateModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public static async Task<ListModelsResponse> ListModels()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, Endpoints.List);
            using HttpResponseMessage httpResp = await HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ListModelsResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }

        public static async Task<ShowModelResponse> ShowModel(ShowModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Show)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.ShowModelRequest)
            };
            using HttpResponseMessage httpResp = await HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.ShowModelResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }

        public static async Task<bool> CopyModel(CopyModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Copy)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.CopyModelRequest)
            };
            using HttpResponseMessage resp = await HttpClient.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public static async Task<bool> DeleteModel(DeleteModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Delete, Endpoints.Delete)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.DeleteModelRequest)
            };
            using HttpResponseMessage resp = await HttpClient.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public static async Task<DelimitedJsonStream<StatusResponse>> PullModelStream(PullModelRequest request)
        {
            using HttpRequestMessage req = new(HttpMethod.Post, Endpoints.Pull)
            {
                Content = JsonContent.Create(request, SourceGenerationContext.Default.PullModelRequest)
            };
            return await GetJsonStream(req, SourceGenerationContext.Default.StatusResponse);
        }

        public static async Task<RunningModelsResponse> ListRunningModels()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, Endpoints.Ps);
            using HttpResponseMessage httpResp = await HttpClient.SendAsync(req);
            if (httpResp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize(await httpResp.Content.ReadAsStringAsync(), SourceGenerationContext.Default.RunningModelsResponse);
            }
            else
            {
                throw new HttpRequestException($"Error: {httpResp.StatusCode} - {await httpResp.Content.ReadAsStringAsync()}");
            }
        }

        public static async Task<VersionResponse> GetVersion()
        {
            using HttpRequestMessage req = new(HttpMethod.Get, Endpoints.Version);
            using HttpResponseMessage httpResp = await HttpClient.SendAsync(req);
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
