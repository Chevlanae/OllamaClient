using OllamaClient.DataContracts;
using OllamaClient.Json;
using System;
using System.Runtime.Serialization;

namespace OllamaClient.Models
{
    public class ChatMessage(Role role, string content, DateTime? timestamp = null) : IChatMessage
    {
        public Role Role { get; set; } = role;
        public string Content { get; set; } = content;
        public DateTime? Timestamp { get; set; } = timestamp;

        public Message ToMessage()
        {
            return new Message()
            {
                role = Role.ToString(),
                content = Content
            };
        }

        public void CopyContract(ChatMessageContract contract)
        {
            Role = contract.Role;
            Content = contract.Content;
            Timestamp = contract.Timestamp;
        }

        public ChatMessageContract ToSerializable()
        {
            return new ()
            {
                Role = Role,
                Content = Content,
                Timestamp = Timestamp
            };
        }
    }
}
