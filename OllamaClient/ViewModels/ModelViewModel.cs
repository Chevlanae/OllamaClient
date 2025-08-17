using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Json;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using System;
using System.ComponentModel;
using System.Text;

namespace OllamaClient.ViewModels
{
    public class ModelViewModel
    {
        private IModel _Model { get; set; }
        private IModelCollection _ModelCollection { get; set; }
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }
        private IDialogsService _DialogsService { get; set; }

        public Paragraph DetailsParagraph { get; set; } = new();
        public Paragraph ModelInfoParagraph { get; set; } = new();
        public Paragraph LicenseParagraph { get; set; } = new();
        public Paragraph ModelFileParagraph { get; set; } = new();

        public ModelViewModel(IModel model, IModelCollection modelCollection, XamlRoot root, DispatcherQueue dispatcherQueue, IDialogsService dialogsService)
        {
            _Model = model;
            _ModelCollection = modelCollection;
            _XamlRoot = root;
            _DispatcherQueue = dispatcherQueue;
            _DialogsService = dialogsService;

            _Model.UnhandledException += Model_UnhandledException;

            if (_Model.LastUpdated == null || _Model.LastUpdated < DateTime.Now.AddMinutes(-5))
            {
                _DispatcherQueue.TryEnqueue(async () =>
                {
                    await _Model.GetDetails();
                    GenerateParagraphText();
                });
            }
            else
            {
                _DispatcherQueue.TryEnqueue(() => GenerateParagraphText());
            }
        }

        public string? Name
        {
            get => _Model.Source?.Name;
        }

        public string? Template
        {
            get => _Model.ModelFile?.Template;
        }

        private void Model_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(_XamlRoot, (Exception)e.ExceptionObject);

            _DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }

        private string GenerateSummaryString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Model: {_Model.Source?.Model}");
            sb.AppendLine($"Modified At: {_Model.Source?.ModifiedAt}");
            sb.AppendLine($"Size: {_Model.Source?.Size / 1024 / 1024} MB");
            sb.AppendLine($"Digest: {_Model.Source?.Digest}");
            if (!string.IsNullOrEmpty(_Model.Source?.ParentModel)) sb.AppendLine($"Parent Model: {_Model.Source?.ParentModel}");
            sb.AppendLine($"Format: {_Model.Source?.Format}");
            sb.AppendLine($"Family: {_Model.Source?.Family}");
            if (_Model.Capabilities is not null) sb.AppendLine($"Capabilities: {string.Join(", ", _Model.Capabilities)}");
            sb.AppendLine($"Parameter Size: {_Model.Source?.ParameterSize}");
            sb.AppendLine($"Quantization Level: {_Model.Source?.QuantizationLevel}");

            return sb.ToString();
        }

        public void GenerateParagraphText()
        {
            DetailsParagraph.Inlines.Clear();
            DetailsParagraph.Inlines.Add(new Run() { Text = GenerateSummaryString() });

            if (_Model.ModelInfo is not null)
            {
                ModelInfoParagraph.Inlines.Clear();
                ModelInfoParagraph.Inlines.Add(new Run() { Text = _Model.ModelInfo });
            }

            LicenseParagraph.Inlines.Clear();
            if (_Model.License is not null)
            {
                LicenseParagraph.Inlines.Add(new Run() { Text = _Model.License });
            }
            else
            {
                LicenseParagraph.Inlines.Add(new Run() { Text = "No license information available." });
            }
            if (_Model.ModelFile is not null)
            {
                ModelFileParagraph.Inlines.Clear();
                ModelFileParagraph.Inlines.Add(new Run() { Text = _Model.ModelFile.ToString() });
            }
        }

        public void ShowDeleteDialog()
        {
            DeleteModelContentDialog dialog = new(_XamlRoot, _Model.Source?.Model ?? "");

            dialog.Closed += (s, args) =>
            {
                if (args.Result == ContentDialogResult.Primary &&  _Model?.Source is not null)
                {
                    _DispatcherQueue.TryEnqueue(async () => { await _ModelCollection.DeleteModel(_Model.Source.Model); });
                }
            };

            _DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }

        public void ShowCopyDialog()
        {
            CopyModelContentDialog dialog = new(_XamlRoot, _Model.Source?.Model ?? "");

            dialog.Closed += (s, args) =>
            {
                if
                (args.Result == ContentDialogResult.Primary && (dialog.Content as TextBoxDialog)?.InputText is string newModelName)
                {
                    _DispatcherQueue?.TryEnqueue(async () => { await _ModelCollection.CopyModel(_Model.Source?.Name ?? "", newModelName); });
                }
            };

            _DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }
    }
}
