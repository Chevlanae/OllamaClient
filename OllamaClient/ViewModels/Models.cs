using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public event EventHandler? ModelFileLoaded;
        public event EventHandler? ModelFileFailed;
        public event UnhandledExceptionEventHandler? UnhandledException;

        protected void OnModelFileLoaded(EventArgs e)
        {
            ModelFileLoaded?.Invoke(this, e);
        }

        protected void OnModelFileFailed(EventArgs e)
        {
            ModelFileFailed?.Invoke(this, e);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(this, e);
        }

        public async Task GetModelDetails()
        {
            try
            {
                ShowModelRequest request = new() { model = Model };
                bool success = false;

                await Task.Run(async () =>
                {
                    if (await Api.ShowModel(request) is ShowModelResponse response)
                    {
                        License = response.license;
                        ModelFile = new(response.modelfile);
                        ModelInfo = JsonSerializer.Serialize(response.model_info, new JsonSerializerOptions { WriteIndented = true });
                        ParentModel = response.details?.parent_model ?? ParentModel;
                        Capabilities = response.capabilities;
                        Tensors = response.tensors;
                        success = true;
                    }
                });

                if (success) OnModelFileLoaded(EventArgs.Empty);
                else OnModelFileFailed(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
            }
        }

        public string ToSummaryString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Model: {Model}");
            sb.AppendLine($"Modified At: {ModifiedAt}");
            sb.AppendLine($"Size: {Size / 1024 / 1024} MB");
            sb.AppendLine($"Digest: {Digest}");
            if(ParentModel is not null and not "") sb.AppendLine($"Parent Model: {ParentModel}");
            sb.AppendLine($"Format: {Format}");
            sb.AppendLine($"Family: {Family}");
            if(Capabilities is not null) sb.AppendLine($"Capabilities: {string.Join(", ", Capabilities)}");
            sb.AppendLine($"Parameter Size: {ParameterSize}");
            sb.AppendLine($"Quantization Level: {QuantizationLevel}");

            return sb.ToString();
        }
    }

    public class ModelCollection
    {
        public ObservableCollection<ModelItem> Items { get; set; } = [];
        public DateTime? LastUpdated { get; set; }
        public List<string> StatusStrings = [];

        public event EventHandler? LoadModelsResponse;
        public event EventHandler? ModelCreated;
        public event EventHandler? ModelDeleted;
        public event EventHandler? ModelCopied;
        public event EventHandler? ModelPulled;
        public event EventHandler? LoadModelsFailed;
        public event EventHandler? ModelCreateFailed;
        public event EventHandler? ModelDeleteFailed;
        public event EventHandler? ModelCopyFailed;
        public event EventHandler? ModelPullFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnLoadModelsResponse(EventArgs e)
        {
            LoadModelsResponse?.Invoke(this, e);
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

        protected void OnLoadModelsFailed(EventArgs e)
        {
            LoadModelsFailed?.Invoke(this, e);
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

                ListModelsResponse? response = null;

                await Task.Run(async () => { response = await Api.ListModels(); });

                if (response is not null)
                {
                    foreach (ModelInfo obj in response.Value.models) Items.Add(new(obj));

                    foreach (ModelItem item in Items) await item.GetModelDetails();

                    LastUpdated = DateTime.Now;
                    OnLoadModelsResponse(EventArgs.Empty);
                }
                else
                {
                    OnLoadModelsFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task CreateModel(string name, string? from, string? system, string? template, string? license, IEnumerable<ModelParameterItem>? parameters)
        {
            try
            {
                List<ModelParameter>? modelParameters = null;
                if (parameters is not null)
                {
                    modelParameters = [];

                    foreach (ModelParameterItem item in parameters)
                    {
                        if (item.Value is not "")
                        {
                            modelParameters.Add(new(item.Key, item.Value));
                        }
                    }
                }

                CreateModelRequest request = new()
                {
                    model = name,
                    from = from,
                    system = system,
                    template = template,
                    license = license,
                    parameters = modelParameters?.ToArray()
                };

                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });

                bool success = false;

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse>? stream = await Api.CreateModelStream(request);

                    if(stream is not null)
                    {
                        await stream.Read(progress, new());
                        success = true;
                    }
                });

                if (success) OnModelCreated(EventArgs.Empty);
                else OnModelCreateFailed(EventArgs.Empty);
            }
            catch(Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
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
                bool success = false;

                await Task.Run(async () => {success = await Api.DeleteModel(new() { model = modelName }); });

                if(success) OnModelDeleted(EventArgs.Empty);
                else OnModelDeleteFailed(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
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
                bool success = false;

                await Task.Run(async () => { success = await Api.CopyModel(new() { source = modelName, destination = newModelName }); });

                if(success) OnModelCopied(EventArgs.Empty);
                else OnModelCopyFailed(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
            }
            finally
            {
                await LoadModels();
            }
        }

        public async Task<ModelDetails?> GetModelDetails(string modelName)
        {
            try
            {
                ModelDetails? result = null;

                await Task.Run(async () => { await Api.ShowModel(new() { model = modelName }); });

                return result;
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
                return null;
            }
        }

        public async Task PullModel(string modelName)
        {
            try
            {
                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });
                bool success = false;

                await Task.Run(async () => 
                { 
                    DelimitedJsonStream<StatusResponse>? stream = await Api.PullModelStream(new() { model = modelName }); 

                    if (stream is not null)
                    {
                        await stream.Read(progress, new());
                        success = true;
                    }
                });

                if(success) OnModelPulled(EventArgs.Empty);
                else OnModelPullFailed(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
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

