using Microsoft.UI.Xaml.Data;
using OllamaClient.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public async Task CreateModel(string model, string? from, string? system, string? template, IEnumerable<ModelParameterItem>? parameterCollection = null)
        {
            try
            {
                CreateModelRequest request = new()
                {
                    model = model,
                    from = from,
                    system = system,
                    template = template
                };

                if (parameterCollection is not null && parameterCollection.Count() > 0)
                {
                    IEnumerable<ModelParameterItem> items = parameterCollection.Where((i) => { return i.Value != null && i.Value != ""; });

                    if (items.Count() > 0)
                    {
                        ModelParameters parameters = new();
                        foreach (ModelParameterItem item in items)
                        {
                            switch (item.Key)
                            {
                                case ModelParameter.mirostat:
                                    parameters.mirostat = int.Parse(item.Value);
                                    break;
                                case ModelParameter.mirostat_eta:
                                    parameters.mirostat_eta = float.Parse(item.Value);
                                    break;
                                case ModelParameter.mirostat_tau:
                                    parameters.mirostat_tau = float.Parse(item.Value);
                                    break;
                                case ModelParameter.temperature:
                                    parameters.temperature = float.Parse(item.Value);
                                    break;
                                case ModelParameter.repeat_last_n:
                                    parameters.repeat_last_n = int.Parse(item.Value);
                                    break;
                                case ModelParameter.repeat_penalty:
                                    parameters.repeat_penalty = float.Parse(item.Value);
                                    break;
                                case ModelParameter.seed:
                                    parameters.seed = int.Parse(item.Value);
                                    break;
                                case ModelParameter.stop:
                                    parameters.stop = item.Value;
                                    break;
                                case ModelParameter.num_predict:
                                    parameters.num_predict = int.Parse(item.Value);
                                    break;
                                case ModelParameter.num_ctx:
                                    parameters.num_ctx = int.Parse(item.Value);
                                    break;
                                case ModelParameter.min_p:
                                    parameters.min_p = float.Parse(item.Value);
                                    break;
                                case ModelParameter.top_k:
                                    parameters.top_k = int.Parse(item.Value);
                                    break;
                                case ModelParameter.top_p:
                                    parameters.top_p = float.Parse(item.Value);
                                    break;
                            }
                        }

                        request.parameters = parameters;
                    }
                }

                IProgress<StatusResponse> progress = new Progress<StatusResponse>((s) => { StatusStrings.Add(s.status); });

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<StatusResponse>? stream = await ApiClient.CreateModelStream(request);

                    if(stream is not null)
                    {
                        await stream.Read(progress, new());
                    }
                });
            }
            catch(FormatException e)
            {
                Debug.Write(e);
            }
            catch(Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new(e, false));
            }
        }
    }

    public class ModelParameterItem : INotifyPropertyChanged
    {
        private ModelParameter K { get; set; }
        private string V { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModelParameter[] KeyOptions => Enum.GetValues<ModelParameter>();

        public ModelParameter Key
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

