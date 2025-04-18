using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Documents;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public class ModelItem(ModelInfo source)
    {
        public string Name { get; set; } = source.name;
        public string Model { get; set; } = source.model;
        public DateTime ModifiedAt { get; set; } = source.modified_at;
        public long Size { get; set; } = source.size;
        public string Digest { get; set; } = source.digest;
        public string? ParentModel { get; set; } = source.details.parent_model;
        public string Format { get; set; } = source.details.format;
        public string Family { get; set; } = source.details.family;
        public string[]? Families { get; set; } = source.details.families;
        public string ParameterSize { get; set; } = source.details.parameter_size;
        public string QuantizationLevel { get; set; } = source.details.quantization_level;

        public string? License { get; set; }
        public ModelFile? ModelFile { get; set; }
        public string? ModelInfo { get; set; }
        public string[]? Capabilities { get; set; }
        public TensorInfo[]? Tensors { get; set; }

        public Paragraph DetailsParagraph { get; set; } = new();
        public Paragraph ModelInfoParagraph { get; set; } = new();
        public Paragraph LicenseParagraph { get; set; } = new();
        public Paragraph ModelFileParagraph { get; set; } = new();

        public event EventHandler? ShowModelinfoLoaded;
        public event EventHandler? ShowModelinfoFailed;
        public event UnhandledExceptionEventHandler? UnhandledException;

        protected void OnShowModelinfoLoaded(EventArgs e)
        {
            ShowModelinfoLoaded?.Invoke(this, e);
        }

        protected void OnShowModelinfoFailed(EventArgs e)
        {
            ShowModelinfoFailed?.Invoke(this, e);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(this, e);
        }

        public async Task GetShowModelInfo()
        {
            try
            {
                await Task.Run(async () =>
                {
                    ShowModelResponse response = await Api.ShowModel(new() { model = Model });

                    License = response.license;
                    ModelFile = new(response.modelfile);
                    ModelInfo = JsonSerializer.Serialize(response.model_info, new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 });
                    ParentModel = response.details?.parent_model ?? ParentModel;
                    Capabilities = response.capabilities;
                    Tensors = response.tensors;
                });

                OnShowModelinfoLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to load model info for {Model}", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
                OnShowModelinfoFailed(EventArgs.Empty);
            }
        }

        private string ToSummaryString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Model: {Model}");
            sb.AppendLine($"Modified At: {ModifiedAt}");
            sb.AppendLine($"Size: {Size / 1024 / 1024} MB");
            sb.AppendLine($"Digest: {Digest}");
            if (!string.IsNullOrEmpty(ParentModel)) sb.AppendLine($"Parent Model: {ParentModel}");
            sb.AppendLine($"Format: {Format}");
            sb.AppendLine($"Family: {Family}");
            if (Capabilities is not null) sb.AppendLine($"Capabilities: {string.Join(", ", Capabilities)}");
            sb.AppendLine($"Parameter Size: {ParameterSize}");
            sb.AppendLine($"Quantization Level: {QuantizationLevel}");

            return sb.ToString();
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
    }
}
