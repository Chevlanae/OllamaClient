using OllamaClient.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
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

        public string ToDetailsString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Model: {Model}");
            sb.AppendLine($"Modified At: {ModifiedAt}");
            sb.AppendLine($"Size: {Size / 1024 / 1024} MB");
            sb.AppendLine($"Digest: {Digest}");
            sb.AppendLine($"Format: {Format}");
            sb.AppendLine($"Family: {Family}");
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

        public event EventHandler<EventArgs>? LoadModelsResponse;
        public event EventHandler? ModelCreated;
        public event EventHandler? ModelDeleted;
        public event EventHandler? ModelCopied;
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
                    foreach (ModelInfo obj in response.Value.models)
                    {
                        Items.Add(new(obj));
                    }

                    LastUpdated = DateTime.Now;
                }

                OnLoadModelsResponse(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task CreateModel(string name, ModelFile modelFile)
        {
            await CreateModel(name, modelFile.From, modelFile.System, modelFile.Template, modelFile.License, modelFile.Parameters);
        }

        public async Task CreateModel(string name, string? from, string? system, string? template, string? license, ModelParameters? parameters)
        {
            try
            {
                CreateModelRequest request = new()
                {
                    model = name,
                    from = from,
                    system = system,
                    template = template,
                    license = license,
                    parameters = parameters
                };

                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse>? stream = await Api.CreateModelStream(request);

                    if(stream is not null)
                    {
                        await stream.Read(progress, new());
                    }
                });

                OnModelCreated(EventArgs.Empty);
            }
            catch(FormatException e)
            {
                Debug.Write(e);
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
                await Task.Run(async () => { await Api.DeleteModel(new() { model = modelName }); });

                OnModelDeleted(EventArgs.Empty);
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
                await Task.Run(async () => { await Api.CopyModel(new() { source = modelName, destination = newModelName }); });

                OnModelCopied(EventArgs.Empty);
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

                await Task.Run(async () => 
                { 
                    DelimitedJsonStream<StatusResponse>? stream = await Api.PullModelStream(new() { model = modelName }); 

                    if (stream is not null)
                    {
                        await stream.Read(progress, new());
                    }
                });
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

