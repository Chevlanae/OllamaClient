using Microsoft.Extensions.Logging;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public class ModelCollection
    {
        public ObservableCollection<ModelItem> Items { get; set; } = [];
        public DateTime? LastUpdated { get; set; }
        public List<string> StatusStrings = [];

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

        protected void OnModelsLoaded(EventArgs e)
        {
            ModelsLoaded?.Invoke(this, e);
        }

        protected void OnModelCreated(EventArgs e)
        {
            ModelCreated?.Invoke(this, e);
        }

        protected void OnModelDeleted(EventArgs e)
        {
            ModelDeleted?.Invoke(this, e);
        }

        protected void OnModelCopied(EventArgs e)
        {
            ModelCopied?.Invoke(this, e);
        }

        protected void OnModelPulled(EventArgs e)
        {
            ModelPulled?.Invoke(this, e);
        }

        protected void OnModelsLoadFailed(EventArgs e)
        {
            ModelsLoadFailed?.Invoke(this, e);
        }

        protected void OnModelCreateFailed(EventArgs e)
        {
            ModelCreateFailed?.Invoke(this, e);
        }

        protected void OnModelDeleteFailed(EventArgs e)
        {
            ModelDeleteFailed?.Invoke(this, e);
        }

        protected void OnModelCopyFailed(EventArgs e)
        {
            ModelCopyFailed?.Invoke(this, e);
        }

        protected void OnModelPullFailed(EventArgs e)
        {
            ModelPullFailed?.Invoke(this, e);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(this, e);
        }

        public async Task LoadModels()
        {
            try
            {
                Items.Clear();

                ListModelsResponse response = await Task.Run(Api.ListModels);

                foreach (ModelInfo obj in response.models) Items.Add(new(obj));

                foreach (ModelItem item in Items) await item.GetShowModelInfo();

                LastUpdated = DateTime.Now;

                OnModelsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to load model list", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task CreateModel(string name, string? from, string? system, string? template, string? license, IEnumerable<ModelParameterItem>? parameters)
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
                template = template,
                license = license
            };

            if (parameters is not null)
            {
                request.parameters = [];

                foreach (ModelParameterItem item in parameters.Where(p => p.Value is not null && p.Value != string.Empty))
                {
                    request.parameters[item.Key] = item.Value;
                }
            }

            IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });

            try
            {
                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse>? stream = await Api.CreateModelStream(request);

                    await stream.Read(progress, new());
                });

                OnModelCreated(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to create model {name}", LogLevel.Error, e);
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
                if (await Task.Run(async () => { return await Api.DeleteModel(new() { model = modelName }); }))
                {
                    OnModelDeleted(EventArgs.Empty);
                }
                else
                {
                    OnModelDeleteFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to delete model {modelName}", LogLevel.Error, e);
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
                if (await Task.Run(async () => { return await Api.CopyModel(new() { source = modelName, destination = newModelName }); }))
                {
                    OnModelCopied(EventArgs.Empty);
                }
                else
                {
                    OnModelCopyFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to copy model {modelName} to {newModelName}", LogLevel.Error, e);
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
                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse> stream = await Api.PullModelStream(new() { model = modelName });

                    await stream.Read(progress, new());
                });

                OnModelPulled(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to pull model {modelName}", LogLevel.Error, e);
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

