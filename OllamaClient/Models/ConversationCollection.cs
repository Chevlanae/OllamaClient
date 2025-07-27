using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    [KnownType(typeof(ChatMessage))]
    [KnownType(typeof(Conversation))]
    [DataContract]
    public class ConversationCollection
    {
        [DataMember]
        public ObservableCollection<Conversation> Items { get; set; } = [];
    }
}
