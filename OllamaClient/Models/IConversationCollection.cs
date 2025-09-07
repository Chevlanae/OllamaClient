using OllamaClient.Services.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public interface IConversationCollection
    {
        List<string> AvailableModels { get; set; }
        List<IConversation> Items { get; set; }
        DateTime? LastUpdated { get; set; }

        event EventHandler? ConversationsLoaded;
        event EventHandler? ConversationsLoadFailed;
        event EventHandler? ModelsLoaded;
        event EventHandler? ModelsLoadFailed;
        event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        void CopyContract(ConversationCollectionContract contract);
        Task<bool> LoadAvailableModels();
        Task<bool> LoadConversations();
        Task<bool> Save();
        ConversationCollectionContract ToSerializable();
    }
}