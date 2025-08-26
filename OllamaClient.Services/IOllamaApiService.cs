using OllamaClient.Services.Json;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    public interface IOllamaApiService
    {
        Task<DelimitedJsonStream<ChatResponse>> ChatStream(ChatRequest request);
        Task<DelimitedJsonStream<CompletionResponse>> CompletionStream(CompletionRequest request);
        Task<bool> CopyModel(CopyModelRequest request);
        Task<DelimitedJsonStream<StatusResponse>> CreateModelStream(CreateModelRequest request);
        Task<bool> DeleteModel(DeleteModelRequest request);
        Task<VersionResponse> GetVersion();
        Task<ListModelsResponse> ListModels();
        Task<RunningModelsResponse> ListRunningModels();
        Task<DelimitedJsonStream<StatusResponse>> PullModelStream(PullModelRequest request);
        Task<ShowModelResponse> ShowModel(ShowModelRequest request);
    }
}