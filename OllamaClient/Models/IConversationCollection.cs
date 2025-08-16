using OllamaClient.DataContracts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OllamaClient.Models
{
    public interface IConversationCollection
    {
        public List<IConversation> Items { get; set; }
        public List<string> AvailableModels { get; set; }
        public DateTime? LastUpdated { get; set; }

        public void CopyContract(ConversationCollectionContract contract);
        public ConversationCollectionContract ToSerializable();
    }
}