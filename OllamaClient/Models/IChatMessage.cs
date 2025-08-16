using OllamaClient.DataContracts;
using OllamaClient.Json;
using System;

namespace OllamaClient.Models
{
    public interface IChatMessage
    {
        public string Content { get; set; }
        public Role Role { get; set; }
        public DateTime? Timestamp { get; set; }

        public Message ToMessage();

        public void CopyContract(ChatMessageContract contract);
        public ChatMessageContract ToSerializable();
    }
}