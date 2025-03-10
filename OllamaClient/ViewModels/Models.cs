using OllamaClient.Models.Ollama;
using System;
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

    public class ModelCollection(Client? connection = null)
    {
        private Client OllamaConnection { get; set; } = connection ?? new();
        public ObservableCollection<ModelItem> Items { get; set; } = [];
        public DateTime? LastUpdated { get; set; }

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

                await Task.Run(async () => { response = await OllamaConnection.ListModels(); });

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
    }
}
