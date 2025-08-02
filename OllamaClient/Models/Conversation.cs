using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OllamaClient.Models
{
    [KnownType(typeof(ChatMessage))]
    [DataContract]
    public class Conversation
    {
        [DataMember]
        public List<ChatMessage> ChatMessageCollection { get; set; } = [];
        [DataMember]
        public string? Subject { get; set; }
        [DataMember]
        public string? SelectedModel { get; set; }
    }
}
