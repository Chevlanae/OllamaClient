using OllamaClient.DataContracts;
using System.Collections.Generic;

namespace OllamaClient.Models
{
    public interface IConversation
    {
        public List<IChatMessage> ChatMessageCollection { get; set; }
        public string? SelectedModel { get; set; }
        public string? Subject { get; set; }

        public void CopyContract(ConversationContract conversation);
        public ConversationContract ToSerializable();
    }
}