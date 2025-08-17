using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public interface IModelCollection
    {
        List<IModel> Items { get; set; }
        DateTime? LastUpdated { get; set; }

        event EventHandler? ModelCopied;
        event EventHandler? ModelCopyFailed;
        event EventHandler? ModelCreated;
        event EventHandler? ModelCreateFailed;
        event EventHandler? ModelDeleted;
        event EventHandler? ModelDeleteFailed;
        event EventHandler? ModelPulled;
        event EventHandler? ModelPullFailed;
        event EventHandler? ModelsLoaded;
        event EventHandler? ModelsLoadFailed;
        event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        void Cancel();
        Task CopyModel(string modelName, string newModelName);
        Task CreateModel(string name, string? from = null, string? system = null, string? template = null, string? license = null, IEnumerable<ModelParameterViewModel>? parameters = null);
        Task DeleteModel(string modelName);
        Task LoadModels();
        Task PullModel(string modelName);
    }
}