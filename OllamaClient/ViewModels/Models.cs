using OllamaClient.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public class ModelItem(ModelInfo source)
    {
        public string Name { get; set; } = source.name;
        public string Model { get; set; } = source.model;
        public string ModifiedAt { get; set; } = source.modified_at;
        public long Size { get; set; } = source.size;
        public string Digest { get; set; } = source.digest;
        public string? ParentModel { get; set; } = source.details.parent_model;
        public string Format { get; set; } = source.details.format;
        public string Family { get; set; } = source.details.family;
        public string[]? Families { get; set; } = source.details.families;
        public string ParameterSize { get; set; } = source.details.parameter_size;
        public string QuantizationLevel { get; set; } = source.details.quantization_level;
    }

    public class ModelCollection
    {
        public ObservableCollection<ModelItem> Items { get; set; } = [];
        public DateTime? LastUpdated { get; set; }
        public List<string> StatusStrings = [];

        public event EventHandler? ModelsLoaded;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnModelsLoaded(object? sender, EventArgs e)
        {
            ModelsLoaded?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }

        public async Task LoadModels()
        {
            try
            {
                Items.Clear();

                ListModelsResponse? response = null;

                await Task.Run(async () => { response = await ApiClient.ListModels(); });

                if (response is not null)
                {
                    foreach (ModelInfo obj in response.Value.models)
                    {
                        Items.Add(new(obj));
                    }

                    LastUpdated = DateTime.Now;
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new(e, false));
            }
            finally
            {
                OnModelsLoaded(this, EventArgs.Empty);
            }
        }

        public async Task CreateModel(CreateModelRequest request)
        {
            try
            {
                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse>? stream = await ApiClient.CreateModel(request);

                    if(stream is not null)
                    {
                        await stream.Read(progress, new System.Threading.CancellationToken());
                    }
                });
            }
            catch(Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new(e, false));
            }
        }
    }
}
