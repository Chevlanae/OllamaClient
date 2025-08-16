using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Documents;
using OllamaClient.Json;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class Model : IModel
    {
        public class SourceInfo
        {
            public string Name { get; set; }
            public string Model { get; set; }
            public DateTime ModifiedAt { get; set; }
            public long Size { get; set; }
            public string Digest { get; set; }
            public string? ParentModel { get; set; }
            public string Format { get; set; }
            public string Family { get; set; }
            public string[]? Families { get; set; }
            public string ParameterSize { get; set; }
            public string QuantizationLevel { get; set; }

            public SourceInfo(ModelInfo source)
            {
                Name = source.name;
                Model = source.model;
                ModifiedAt = source.modified_at;
                Size = source.size;
                Digest = source.digest;
                ParentModel = source.details.parent_model;
                Format = source.details.format;
                Family = source.details.family;
                Families = source.details.families;
                ParameterSize = source.details.parameter_size;
                QuantizationLevel = source.details.quantization_level;
            }
        }

        private readonly ILogger _Logger;
        private IOllamaApiService _Api;

        public SourceInfo? Source { get; set; }
        public string? License { get; set; }
        public ModelFile? ModelFile { get; set; }
        public string? ModelInfo { get; set; }
        public string[]? Capabilities { get; set; }
        public TensorInfo[]? Tensors { get; set; }
        public DateTime? LastUpdated { get; set; }

        public Model(ILogger<ModelViewModel> logger, IOllamaApiService api)
        {
            _Logger = logger;
            _Api = api;
        }

        public event EventHandler? DetailsLoaded;
        public event EventHandler? DetailsLoadFailed;
        public event UnhandledExceptionEventHandler? UnhandledException;

        protected void OnDetailsLoaded(EventArgs e) => DetailsLoaded?.Invoke(this, e);
        protected void OnDetailsFailed(EventArgs e) => DetailsLoadFailed?.Invoke(this, e);
        protected void OnUnhandledException(UnhandledExceptionEventArgs e) => UnhandledException?.Invoke(this, e);

        public async Task GetDetails()
        {
            if (Source is null) return;

            try
            {
                ShowModelResponse response = await Task.Run(async () => await _Api.ShowModel(new() { model = Source.Model }));

                License = response.license;
                ModelFile = new(response.modelfile);
                ModelInfo = JsonSerializer.Serialize(response.model_info, new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 });
                Source.ParentModel = response.details?.parent_model ?? Source?.ParentModel;
                Capabilities = response.capabilities;
                Tensors = response.tensors;
                LastUpdated = DateTime.Now;
                _Logger.LogInformation("Loaded model info for '{SourceModel}'", Source?.Model);
                OnDetailsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to load model info for '{SourceModel}'", Source?.Model);
                OnUnhandledException(new(e, false));
                OnDetailsFailed(EventArgs.Empty);
            }
        }
    }
}
