using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Documents;
using OllamaClient.Json;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public class ModelViewModel
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
        private OllamaApiService _Api;

        public SourceInfo? Source { get; set; }
        public string? License { get; set; }
        public ModelFile? ModelFile { get; set; }
        public string? ModelInfo { get; set; }
        public string[]? Capabilities { get; set; }
        public TensorInfo[]? Tensors { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Paragraph DetailsParagraph { get; set; } = new();
        public Paragraph ModelInfoParagraph { get; set; } = new();
        public Paragraph LicenseParagraph { get; set; } = new();
        public Paragraph ModelFileParagraph { get; set; } = new();

        public ModelViewModel(ILogger<ModelViewModel> logger)
        {
            _Logger = logger;
            if (App.GetService<OllamaApiService>() is OllamaApiService api)
            {
                _Api = api;
            }
            else throw new ArgumentNullException(nameof(api));
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

        public void GenerateParagraphText()
        {
            DetailsParagraph.Inlines.Clear();
            DetailsParagraph.Inlines.Add(new Run() { Text = ToSummaryString() });

            if (ModelInfo is not null)
            {
                ModelInfoParagraph.Inlines.Clear();
                ModelInfoParagraph.Inlines.Add(new Run() { Text = ModelInfo });
            }

            LicenseParagraph.Inlines.Clear();
            if (License is not null)
            {
                LicenseParagraph.Inlines.Add(new Run() { Text = License });
            }
            else
            {
                LicenseParagraph.Inlines.Add(new Run() { Text = "No license information available." });
            }
            if (ModelFile is not null)
            {
                ModelFileParagraph.Inlines.Clear();
                ModelFileParagraph.Inlines.Add(new Run() { Text = ModelFile.ToString() });
            }
        }

        private string ToSummaryString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Model: {Source?.Model}");
            sb.AppendLine($"Modified At: {Source?.ModifiedAt}");
            sb.AppendLine($"Size: {Source?.Size / 1024 / 1024} MB");
            sb.AppendLine($"Digest: {Source?.Digest}");
            if (!string.IsNullOrEmpty(Source?.ParentModel)) sb.AppendLine($"Parent Model: {Source?.ParentModel}");
            sb.AppendLine($"Format: {Source?.Format}");
            sb.AppendLine($"Family: {Source?.Family}");
            if (Capabilities is not null) sb.AppendLine($"Capabilities: {string.Join(", ", Capabilities)}");
            sb.AppendLine($"Parameter Size: {Source?.ParameterSize}");
            sb.AppendLine($"Quantization Level: {Source?.QuantizationLevel}");

            return sb.ToString();
        }
    }
}
