using Microsoft.Extensions.Logging;
using OllamaClient.Json;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public class ConversationSidebarViewModel
    {
        public ObservableCollection<ConversationViewModel> ConversationViewModels { get; set; } = [];
        public ObservableCollection<string> AvailableModels { get; set; } = [];
        public DateTime? LastUpdated { get; set; }

        private ILogger _Logger;
        private OllamaApiService _Api;
        private SerializeableStorageService _Storage;

        public ConversationSidebarViewModel(ILogger<ConversationSidebarViewModel> logger)
        {
            _Logger = logger;

            if (App.GetService<OllamaApiService>() is OllamaApiService api)
            {
                _Api = api;
            }
            else throw new ArgumentNullException(nameof(api));

            if (App.GetService<SerializeableStorageService>() is SerializeableStorageService storage)
            {
                _Storage = storage;
            }
            else throw new ArgumentNullException(nameof(storage));
        }

        public event EventHandler? ModelsLoaded;
        public event EventHandler? ModelsLoadFailed;
        public event EventHandler? ConversationsLoaded;
        public event EventHandler? ConversationsLoadFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnModelsLoaded(EventArgs e) => ModelsLoaded?.Invoke(this, e);
        protected void OnModelsLoadFailed(EventArgs e) => ModelsLoadFailed?.Invoke(this, e);
        protected void OnConversationsLoaded(EventArgs e) => ConversationsLoaded?.Invoke(this, e);
        protected void OnConversationsLoadFailed(EventArgs e) => ConversationsLoadFailed?.Invoke(this, e);
        protected void OnUnhandledException(UnhandledExceptionEventArgs e) => UnhandledException?.Invoke(this, e);
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
        private async void Conversation_StartOfRequest(object? sender, EventArgs e) => await Save();
        private async void Conversation_EndOfResponse(object? sender, EventArgs e) => await Save();

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
                _Logger.LogError(e, "Failed to load model list");
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task LoadConversations()
        {
            try
            {
                ConversationViewModels.Clear();

                if (await Task.Run(_Storage.Get<ConversationCollection>) is ConversationCollection result)
                {
                    foreach (Conversation c in result.Items)
                    {
                        if (App.GetService<ConversationViewModel>() is ConversationViewModel viewModel)
                        {
                            viewModel.CopyFromConversation(c);
                            ConversationViewModels.Add(viewModel);
                        }
                    }

                    LastUpdated = DateTime.Now;
                    _Logger.LogInformation("Loaded {ItemsCount} conversations", ConversationViewModels.Count);
                    OnConversationsLoaded(EventArgs.Empty);
                }
                else
                {
                    _Logger.LogError("Failed to load data from {ModelName}", nameof(ConversationCollection));
                    OnConversationsLoadFailed(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to load data from {ModelName}", nameof(ConversationCollection));
                OnUnhandledException(new(e, false));
                OnConversationsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task Save()
        {
            try
            {
                ConversationCollection collection = new()
                {
                    Items = ConversationViewModels.Select(vm => vm.ToConversation()).ToList()
                };

                await Task.Run(() => { _Storage.Set(collection); });

                _Logger.LogInformation("Saved {ItemsCount} conversations", collection.Items.Count);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to save {ModelName}", nameof(ConversationCollection));
                OnUnhandledException(new(e, false));
            }
        }
    }
}
