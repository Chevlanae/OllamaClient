using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OllamaClient.Models
{
    [KnownType(typeof(ChatMessage))]
    [DataContract]
    public class Conversation
    {
        [DataMember]
        public ObservableCollection<ChatMessage> ChatMessageCollection { get; set; } = [];
        [DataMember]
        public string? Subject { get; set; }
        [DataMember]
        public string? SelectedModel { get; set; }
    }
}
