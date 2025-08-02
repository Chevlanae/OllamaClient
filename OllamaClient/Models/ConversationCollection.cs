using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OllamaClient.Models
{
    [KnownType(typeof(ChatMessage))]
    [KnownType(typeof(Conversation))]
    [DataContract]
    public class ConversationCollection
    {
        [DataMember]
        public List<Conversation> Items { get; set; } = [];
    }
}
