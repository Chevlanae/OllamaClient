using Microsoft.Extensions.Logging;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    [KnownType(typeof(ChatMessageViewModel))]
    [KnownType(typeof(ConversationViewModel))]
    [DataContract]
    public class ConversationSidebarViewModel : INotifyPropertyChanged
    {
        private ILogger _Logger;
        private OllamaApiService _Api;
        private SerializeableStorageService _Storage;

        [DataMember]
        private ObservableCollection<ConversationViewModel> _ConversationCollection { get; set; } = [];

        public ObservableCollection<string> AvailableModels { get; set; } = [];

        public DateTime? LastUpdated { get; set; }

        public ObservableCollection<ConversationViewModel> Items
        {
            get => _ConversationCollection;
            set
            {
                _ConversationCollection = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler? ModelsLoaded;
        public event EventHandler? ModelsLoadFailed;
        public event EventHandler? ConversationsLoaded;
        public event EventHandler? ConversationsLoadFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ConversationSidebarViewModel(ILogger<ConversationSidebarViewModel> logger)
        {
            _Logger = logger;

            if (App.GetService<OllamaApiService>() is OllamaApiService api)
            {
                _Api = api;
            }
            else throw new ArgumentNullException(nameof(api));

            if(App.GetService<SerializeableStorageService>() is SerializeableStorageService storage)
            {
                _Storage = storage;
            }
            else throw new ArgumentNullException(nameof(storage));
        }

        protected void OnModelsLoaded(EventArgs e)
        {
            ModelsLoaded?.Invoke(this, e);
        }

        protected void OnModelsLoadFailed(EventArgs e)
        {
            ModelsLoadFailed?.Invoke(this, e);
        }

        protected void OnConversationsLoaded(EventArgs e)
        {
            ConversationsLoaded?.Invoke(this, e);
        }

        protected void OnConversationsLoadFailed(EventArgs e)
        {
            ConversationsLoadFailed?.Invoke(this, e);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(this, e);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public async Task LoadAvailableModels()
        {
            try
            {
                string[] results = await Task.Run(async () =>
                {
                    ListModelsResponse response = await _Api.ListModels();

                    return response.models.Select(m => m.model).ToArray();
                });

                AvailableModels = [.. results];

                _Logger.LogInformation("Loaded {AvailableModelsCount} models", AvailableModels.Count);
                OnModelsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                _Logger.LogError($"Failed to load model list", e);
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task LoadConversations()
        {
            try
            {
                Items.Clear();

                if (await Task.Run(_Storage.Get<ConversationSidebarViewModel>) is ConversationSidebarViewModel result)
                {
                    foreach (ConversationViewModel c in result.Items)
                    {
                        Items.Add(c);
                    }

                    LastUpdated = DateTime.Now;
                    _Logger.LogInformation("Loaded {ItemsCount} conversations", Items.Count);
                    OnConversationsLoaded(EventArgs.Empty);
                }
                else
                {
                    _Logger.LogError("Failed to load data from {ViewModelName}", nameof(ConversationSidebarViewModel));
                    OnConversationsLoadFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                _Logger.LogError("Failed to load data from {ViewModelName}", nameof(ConversationSidebarViewModel), e);
                OnUnhandledException(new(e, false));
                OnConversationsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task Save()
        {
            try
            {
                await Task.Run(() => { _Storage.Set(this); });
                _Logger.LogInformation("Saved {ItemsCount} conversations", Items.Count);
            }
            catch (Exception e)
            {
                _Logger.LogError("Failed to save {ViewModelName}", nameof(ConversationSidebarViewModel), e);
                OnUnhandledException(new(e, false));
            }
        }
    }
}
