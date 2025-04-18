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
    [KnownType(typeof(ChatItem))]
    [KnownType(typeof(Conversation))]
    [DataContract]
    public class Conversations : INotifyPropertyChanged
    {
        [DataMember]
        private ObservableCollection<Conversation> ConversationCollection { get; set; } = [];

        public ObservableCollection<string> AvailableModels { get; set; } = [];

        public ObservableCollection<Conversation> Items
        {
            get => ConversationCollection;
            set
            {
                ConversationCollection = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler? ModelsLoaded;
        public event EventHandler? ModelsLoadFailed;
        public event EventHandler? ConversationsLoaded;
        public event EventHandler? ConversationsLoadFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;
        public event PropertyChangedEventHandler? PropertyChanged;

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

        public async Task LoadModels()
        {
            try
            {
                string[] results = await Task.Run(async () =>
                {
                    ListModelsResponse response = await Api.ListModels();

                    return response.models.Select(m => m.model).ToArray();
                });

                AvailableModels = [.. results];

                OnModelsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to load model list", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task LoadConversations()
        {
            try
            {
                Items.Clear();

                if (await Task.Run(LocalStorage.Get<Conversations>) is Conversations result)
                {
                    foreach (Conversation c in result.Items)
                    {
                        Items.Add(c);
                    }
                }

                OnConversationsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to load saved conversations", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
                OnConversationsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task Save()
        {
            try
            {
                await Task.Run(() => { LocalStorage.Set(this); });
            }
            catch (Exception e)
            {
                Logging.Log($"Failed to save conversations", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
            }
        }
    }
}
