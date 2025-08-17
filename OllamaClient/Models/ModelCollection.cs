using Microsoft.Extensions.Logging;
using OllamaClient.Json;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class ModelCollection : IModelCollection
    {
        private readonly ILogger _Logger;
        private readonly IOllamaApiService _Api;
        private CancellationTokenSource _CancellationTokenSource { get; set; } = new();

        public List<IModel> Items { get; set; } = [];
        public DateTime? LastUpdated { get; set; }

        public ModelCollection(ILogger<ModelCollection> logger, IOllamaApiService api)
        {
            _Logger = logger;
            _Api = api;
        }

        public event EventHandler? ModelsLoaded;
        public event EventHandler? ModelCreated;
        public event EventHandler? ModelDeleted;
        public event EventHandler? ModelCopied;
        public event EventHandler? ModelPulled;
        public event EventHandler? ModelsLoadFailed;
        public event EventHandler? ModelCreateFailed;
        public event EventHandler? ModelDeleteFailed;
        public event EventHandler? ModelCopyFailed;
        public event EventHandler? ModelPullFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnModelsLoaded(EventArgs e) => ModelsLoaded?.Invoke(this, e);
        protected void OnModelCreated(EventArgs e) => ModelCreated?.Invoke(this, e);
        protected void OnModelDeleted(EventArgs e) => ModelCreated?.Invoke(this, e);
        protected void OnModelCopied(EventArgs e) => ModelCopied?.Invoke(this, e);
        protected void OnModelPulled(EventArgs e) => ModelPulled?.Invoke(this, e);
        protected void OnModelsLoadFailed(EventArgs e) => ModelsLoadFailed?.Invoke(this, e);
        protected void OnModelCreateFailed(EventArgs e) => ModelCreateFailed?.Invoke(this, e);
        protected void OnModelDeleteFailed(EventArgs e) => ModelDeleteFailed?.Invoke(this, e);
        protected void OnModelCopyFailed(EventArgs e) => ModelCopyFailed?.Invoke(this, e);
        protected void OnModelPullFailed(EventArgs e) => ModelPullFailed?.Invoke(this, e);
        protected void OnUnhandledException(UnhandledExceptionEventArgs e) => UnhandledException?.Invoke(this, e);

        public void Cancel()
        {
            _CancellationTokenSource.Cancel();
            _CancellationTokenSource = new();
        }

        public async Task LoadModels()
        {
            try
            {
                Items.Clear();

                ListModelsResponse response = await Task.Run(_Api.ListModels);

                foreach (ModelInfo obj in response.models)
                {
                    IModel model = App.GetRequiredService<IModel>();
                    model.Source = new(obj);
                    Items.Add(model);
                }

                LastUpdated = DateTime.Now;
                _Logger.LogInformation("Loaded {ItemsCount} models", Items.Count);
                OnModelsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                _Logger.LogError($"Failed to load model list", e);
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task CreateModel(
            string name,
            string? from = null,
            string? system = null,
            string? template = null,
            string? license = null,
            IEnumerable<ModelParameterViewModel>? parameters = null)
        {
            if (from == string.Empty || system == string.Empty || template == string.Empty)
            {
                OnModelCreateFailed(EventArgs.Empty);
                return;
            }

            CreateModelRequest request = new()
            {
                model = name,
                from = from,
                system = system,
                template = template?.Trim(),
                license = license
            };

            IEnumerable<IModelParameter>? resolvedParameters = parameters?.Where
            (
                p =>
                p.Value is not null
                &&
                p.Value != string.Empty
            );

            if (resolvedParameters is not null && resolvedParameters.Count() > 0)
            {
                request.parameters = [];

                foreach (ModelParameterViewModel item in resolvedParameters)
                {
                    request.parameters[item.Key] = item.Value;
                }
            }

            IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) =>
            {
                _Logger.LogInformation("Creating '{Name}' - {Status}", name, s.status);
            });

            try
            {
                await Task.Run(async () =>
                {
                    using DelimitedJsonStream<StatusResponse>? stream = await _Api.CreateModelStream(request);

                    await stream.Read(progress, _CancellationTokenSource.Token);
                });

                OnModelCreated(EventArgs.Empty);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to create model '{Name}'", name);
                OnUnhandledException(new(e, false));
                OnModelCreateFailed(EventArgs.Empty);
            }
            finally
            {
                await LoadModels();
            }
        }

        public async Task DeleteModel(string modelName)
        {
            try
            {
                if (await Task.Run(async () => { return await _Api.DeleteModel(new() { model = modelName }); }))
                {
                    _Logger.LogInformation("Model '{ModelName}' deleted successfully", modelName);
                    OnModelDeleted(EventArgs.Empty);
                }
                else
                {
                    _Logger.LogInformation("Failed to delete model '{ModelName}'", modelName);
                    OnModelDeleteFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to delete model '{ModelName}'", modelName);
                OnUnhandledException(new(e, false));
                OnModelDeleteFailed(EventArgs.Empty);
            }
            finally
            {
                await LoadModels();
            }
        }

        public async Task CopyModel(string modelName, string newModelName)
        {
            try
            {
                if (await Task.Run(async () => { return await _Api.CopyModel(new() { source = modelName, destination = newModelName }); }))
                {
                    _Logger.LogInformation("Model '{ModelName}' copied to '{NewModelName}' successfully", modelName, newModelName);
                    OnModelCopied(EventArgs.Empty);
                }
                else
                {
                    _Logger.LogInformation("Failed to copy model '{ModelName}' to '{NewModelName}'", modelName, newModelName);
                    OnModelCopyFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to copy model '{ModelName}' to '{NewModelName}'", modelName, newModelName);
                OnUnhandledException(new(e, false));
                OnModelCopyFailed(EventArgs.Empty);
            }
            finally
            {
                await LoadModels();
            }
        }

        public async Task PullModel(string modelName)
        {
            try
            {
                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) =>
                {
                    if (s.total is null)
                    {
                        _Logger.LogInformation("Pulling '{ModelName}' - {Status}", modelName, s.status);
                    }
                    else
                    {
                        _Logger.LogInformation("Pulling '{ModelName}' - {Status} - {Completed}/{Total} bytes downloaded ", modelName, s.status, s.completed ?? 0, s.total);
                    }
                });

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse> stream = await _Api.PullModelStream(new() { model = modelName });

                    await stream.Read(progress, _CancellationTokenSource.Token);
                });

                OnModelPulled(EventArgs.Empty);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to pull model '{ModelName}'", modelName);
                OnUnhandledException(new(e, false));
                OnModelPullFailed(EventArgs.Empty);
            }
            finally
            {
                await LoadModels();
            }
        }
    }
}
