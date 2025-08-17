using Microsoft.Extensions.Logging;
using OllamaClient.DataContracts;
using OllamaClient.Json;
using OllamaClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class ConversationCollection : IConversationCollection
    {
        public List<IConversation> Items { get; set; } = [];
        public List<string> AvailableModels { get; set; } = [];
        public DateTime? LastUpdated { get; set; }

        private readonly ILogger _Logger;
        private readonly IOllamaApiService _Api;
        private readonly ISerializeableStorageService _Storage;

        public ConversationCollection(ILogger<IConversationCollection> logger, IOllamaApiService api, ISerializeableStorageService storage)
        {
            _Logger = logger;
            _Api = api;
            _Storage = storage;
        }

        public event EventHandler? ModelsLoaded;
        public event EventHandler? ModelsLoadFailed;
        public event EventHandler? ConversationsLoaded;
        public event EventHandler? ConversationsLoadFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnModelsLoaded(EventArgs e) => ModelsLoaded?.Invoke(this, e);
        protected void OnModelsLoadFailed(EventArgs e) => ModelsLoadFailed?.Invoke(this, e);
        protected void OnConversationsLoaded(EventArgs e) => ConversationsLoaded?.Invoke(this, e);
        protected void OnConversationsLoadFailed(EventArgs e) => ConversationsLoadFailed?.Invoke(this, e);
        protected void OnUnhandledException(UnhandledExceptionEventArgs e) => UnhandledException?.Invoke(this, e);
        private async void Conversation_StartOfRequest(object? sender, EventArgs e) => await Save();
        private async void Conversation_EndOfResponse(object? sender, EventArgs e) => await Save();

        public void CopyContract(ConversationCollectionContract contract)
        {
            try
            {
                Items = new List<IConversation>();
                foreach (ConversationContract conversation in contract.Items)
                {
                    IConversation newConversation = App.GetRequiredService<IConversation>();
                    newConversation.CopyContract(conversation);
                    Items.Add(newConversation);
                }
                AvailableModels = contract.AvailableModels;
                LastUpdated = contract.LastUpdated;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to copy data from contract");
                OnUnhandledException(new(e, true));
            }
        }

        public ConversationCollectionContract ToSerializable()
        {
            return new()
            {
                Items = Items.Select(c => c.ToSerializable()).ToList(),
                AvailableModels = AvailableModels,
                LastUpdated = LastUpdated,
            };
        }

        public async Task<bool> LoadAvailableModels()
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
                return true;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to load model list");
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
                return false;
            }
        }

        public async Task<bool> LoadConversations()
        {
            try
            {
                Items.Clear();

                if (await Task.Run(_Storage.Get<ConversationCollectionContract>) is ConversationCollectionContract result)
                {
                    CopyContract(result);
                    _Logger.LogInformation("Loaded {ItemsCount} conversations", Items.Count);
                    OnConversationsLoaded(EventArgs.Empty);
                    return true;
                }
                else
                {
                    _Logger.LogError("Failed to load data from {ModelName}", nameof(ConversationCollectionContract));
                    OnConversationsLoadFailed(EventArgs.Empty);
                    return false;
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to load data from {ModelName}", nameof(ConversationCollectionContract));
                OnUnhandledException(new(e, false));
                OnConversationsLoadFailed(EventArgs.Empty);
                return false;
            }
        }

        public async Task<bool> Save()
        {
            try
            {
                ConversationCollectionContract contract = ToSerializable();

                await Task.Run(() => { _Storage.Set(contract); });
                _Logger.LogInformation("Saved {ItemsCount} conversations", Items.Count);
                return true;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to save {ModelName}", nameof(ConversationCollection));
                OnUnhandledException(new(e, false));
                return false;
            }
        }
    }
}
