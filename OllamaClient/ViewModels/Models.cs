using Microsoft.UI.Xaml.Documents;
using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
                Debug.Write(e);
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
            if(!string.IsNullOrEmpty(ParentModel)) sb.AppendLine($"Parent Model: {ParentModel}");
            sb.AppendLine($"Format: {Format}");
            sb.AppendLine($"Family: {Family}");
            if(Capabilities is not null) sb.AppendLine($"Capabilities: {string.Join(", ", Capabilities)}");
            sb.AppendLine($"Parameter Size: {ParameterSize}");
            sb.AppendLine($"Quantization Level: {QuantizationLevel}");

            return sb.ToString();
        }

        public void GenerateParagraphText()
        {
            DetailsParagraph.Inlines.Clear();
            DetailsParagraph.Inlines.Add(new Run() { Text = ToSummaryString() });

            if(ModelInfo is not null)
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
                Debug.Write(e);
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task CreateModel(string name, string? from, string? system, string? template, string? license, IEnumerable<ModelParameterItem>? parameters)
        {
            if(from == string.Empty || system == string.Empty || template == string.Empty)
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
                request.parameters = new();

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
            catch(Exception e)
            {
                Debug.Write(e);
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
                if(await Task.Run(async () => { return await Api.DeleteModel(new() { model = modelName }); }))
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
                Debug.Write(e);
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
                if(await Task.Run(async () => { return await Api.CopyModel(new() { source = modelName, destination = newModelName }); }))
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
                Debug.Write(e);
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
                Debug.Write(e);
                OnUnhandledException(new(e, false));
                OnModelPullFailed(EventArgs.Empty);
            }
            finally
            {
                await LoadModels();
            }
        }
    }

    public class ModelParameterItem : IModelParameter, INotifyPropertyChanged
    {
        private ModelParameterKey K { get; set; }
        private string V { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModelParameterKey[] KeyOptions => Enum.GetValues<ModelParameterKey>();

        public ModelParameterKey Key
        {
            get => K;
            set
            {
                K = value;
                OnPropertyChanged();
            }
        }
        public string Value
        {
            get => V;
            set
            {
                V = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new (propertyName));
        }
    }
}

