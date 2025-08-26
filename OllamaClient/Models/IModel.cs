using OllamaClient.Services.Json;
using System;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public interface IModel
    {
        string[]? Capabilities { get; set; }
        DateTime? LastUpdated { get; set; }
        string? License { get; set; }
        ModelFile? ModelFile { get; set; }
        string? ModelInfo { get; set; }
        Model.SourceInfo? Source { get; set; }
        TensorInfo[]? Tensors { get; set; }

        event EventHandler? DetailsLoaded;
        event EventHandler? DetailsLoadFailed;
        event UnhandledExceptionEventHandler? UnhandledException;

        Task GetDetails();
    }
}